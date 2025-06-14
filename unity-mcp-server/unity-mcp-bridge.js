#!/usr/bin/env node

const { Server } = require('@modelcontextprotocol/sdk/server/index.js');
const { StdioServerTransport } = require('@modelcontextprotocol/sdk/server/stdio.js');
const {
  CallToolRequestSchema,
  ErrorCode,
  ListToolsRequestSchema,
  McpError,
} = require('@modelcontextprotocol/sdk/types.js');
const axios = require('axios');

class UnityMCPServer {
  constructor() {
    this.server = new Server(
      {
        name: 'unity-compiler',
        version: '0.1.0',
      },
      {
        capabilities: {
          tools: {},
        },
      }
    );

    this.unityHost = process.env.UNITY_MCP_HOST || 'localhost';
    this.unityPort = process.env.UNITY_MCP_PORT || '8080';
    this.unityBaseUrl = `http://${this.unityHost}:${this.unityPort}`;

    this.setupToolHandlers();
  }

  setupToolHandlers() {
    this.server.setRequestHandler(ListToolsRequestSchema, async () => {
      return {
        tools: [
          {
            name: 'unity_compile',
            description: 'Trigger Unity script compilation',
            inputSchema: {
              type: 'object',
              properties: {},
              required: [],
            },
          },
          {
            name: 'unity_get_compile_status',
            description: 'Get current Unity compilation status including errors and warnings',
            inputSchema: {
              type: 'object',
              properties: {},
              required: [],
            },
          },
          {
            name: 'unity_get_diagnostics',
            description: 'Get detailed compilation diagnostics including error messages and file locations',
            inputSchema: {
              type: 'object',
              properties: {},
              required: [],
            },
          },
          {
            name: 'unity_get_project_status',
            description: 'Get comprehensive Unity project compilation status including overall health, all errors/warnings, and summary statistics',
            inputSchema: {
              type: 'object',
              properties: {},
              required: [],
            },
          },
        ],
      };
    });

    this.server.setRequestHandler(CallToolRequestSchema, async (request) => {
      const { name, arguments: args } = request.params;

      try {
        switch (name) {
          case 'unity_compile':
            return await this.triggerCompilation();
          case 'unity_get_compile_status':
            return await this.getCompileStatus();
          case 'unity_get_diagnostics':
            return await this.getDiagnostics();
          case 'unity_get_project_status':
            return await this.getProjectStatus();
          default:
            throw new McpError(
              ErrorCode.MethodNotFound,
              `Unknown tool: ${name}`
            );
        }
      } catch (error) {
        if (error instanceof McpError) {
          throw error;
        }
        
        // Handle Unity server connection errors
        if (error.code === 'ECONNREFUSED') {
          throw new McpError(
            ErrorCode.InternalError,
            `Unity MCP Server not running. Please start Unity Editor and open Tools > Unity MCP Server, then click 'Start Server'.`
          );
        }
        
        throw new McpError(
          ErrorCode.InternalError,
          `Error calling Unity API: ${error.message}`
        );
      }
    });
  }

  async triggerCompilation() {
    try {
      const response = await axios.post(`${this.unityBaseUrl}/api/compile`, {}, {
        timeout: 5000,
        headers: { 'Content-Type': 'application/json' }
      });
      
      return {
        content: [
          {
            type: 'text',
            text: `Unity compilation triggered successfully.\nStatus: ${response.data.status}\nTimestamp: ${response.data.timestamp}`,
          },
        ],
      };
    } catch (error) {
      if (error.response) {
        return {
          content: [
            {
              type: 'text',
              text: `Unity compilation trigger failed: ${error.response.status} ${error.response.statusText}`,
            },
          ],
          isError: true,
        };
      }
      throw error;
    }
  }

  async getCompileStatus() {
    try {
      const response = await axios.get(`${this.unityBaseUrl}/api/compile_status`, {
        timeout: 5000
      });
      
      const status = response.data;
      
      return {
        content: [
          {
            type: 'text',
            text: `Unity Compilation Status:
Status: ${status.status}
Duration: ${status.duration_ms}ms
Errors: ${status.error_count}
Warnings: ${status.warning_count}
Last Updated: ${status.timestamp}`,
          },
        ],
      };
    } catch (error) {
      if (error.response) {
        return {
          content: [
            {
              type: 'text',
              text: `Failed to get Unity compile status: ${error.response.status} ${error.response.statusText}`,
            },
          ],
          isError: true,
        };
      }
      throw error;
    }
  }

  async getDiagnostics() {
    try {
      const response = await axios.get(`${this.unityBaseUrl}/api/diagnostics`, {
        timeout: 5000
      });
      
      const diagnostics = response.data;
      
      if (!diagnostics.messages || diagnostics.messages.length === 0) {
        return {
          content: [
            {
              type: 'text',
              text: 'No compilation diagnostics available.',
            },
          ],
        };
      }
      
      const messages = diagnostics.messages.map(msg => 
        `[${msg.type.toUpperCase()}] ${msg.message}${msg.file ? ` (${msg.file}:${msg.line})` : ''}`
      ).join('\n');
      
      return {
        content: [
          {
            type: 'text',
            text: `Unity Compilation Diagnostics:\n\n${messages}`,
          },
        ],
      };
    } catch (error) {
      if (error.response) {
        return {
          content: [
            {
              type: 'text',
              text: `Failed to get Unity diagnostics: ${error.response.status} ${error.response.statusText}`,
            },
          ],
          isError: true,
        };
      }
      throw error;
    }
  }

  async getProjectStatus() {
    try {
      // Get both compile status and diagnostics for comprehensive view
      const [statusResponse, diagnosticsResponse] = await Promise.all([
        axios.get(`${this.unityBaseUrl}/api/compile_status`, { timeout: 5000 }),
        axios.get(`${this.unityBaseUrl}/api/diagnostics`, { timeout: 5000 })
      ]);
      
      const status = statusResponse.data;
      const diagnostics = diagnosticsResponse.data;
      
      // Categorize messages
      const errors = diagnostics.messages?.filter(msg => msg.type === 'error') || [];
      const warnings = diagnostics.messages?.filter(msg => msg.type === 'warning') || [];
      
      // Determine overall health
      const health = errors.length > 0 ? '❌ ERRORS' : 
                    warnings.length > 0 ? '⚠️ WARNINGS' : '✅ CLEAN';
      
      let statusText = `Unity Project Status Report
==========================================
Overall Health: ${health}
Compilation Status: ${status.status}
Duration: ${status.duration_ms}ms
Last Updated: ${status.timestamp}

Statistics:
• Errors: ${errors.length}
• Warnings: ${warnings.length}
• Total Issues: ${errors.length + warnings.length}`;

      if (errors.length > 0) {
        statusText += `\n\n🚨 ERRORS (${errors.length}):\n`;
        statusText += errors.map(err => 
          `  • ${err.message}${err.file ? ` (${err.file}:${err.line})` : ''}`
        ).join('\n');
      }
      
      if (warnings.length > 0) {
        statusText += `\n\n⚠️ WARNINGS (${warnings.length}):\n`;
        statusText += warnings.map(warn => 
          `  • ${warn.message}${warn.file ? ` (${warn.file}:${warn.line})` : ''}`
        ).join('\n');
      }
      
      if (errors.length === 0 && warnings.length === 0) {
        statusText += '\n\n🎉 No compilation issues detected!';
      }
      
      return {
        content: [
          {
            type: 'text',
            text: statusText,
          },
        ],
      };
    } catch (error) {
      if (error.response) {
        return {
          content: [
            {
              type: 'text',
              text: `Failed to get Unity project status: ${error.response.status} ${error.response.statusText}`,
            },
          ],
          isError: true,
        };
      }
      throw error;
    }
  }

  async run() {
    const transport = new StdioServerTransport();
    await this.server.connect(transport);
    console.error('Unity MCP Server running on stdio');
  }
}

const server = new UnityMCPServer();
server.run().catch(console.error);