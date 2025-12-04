/**
 * EchoBooster Node.js Server
 * Provides API endpoints for system monitoring and performance optimization
 */

const express = require('express');
const http = require('http');
const socketIo = require('socket.io');
const si = require('systeminformation');
const cors = require('cors');
const bodyParser = require('body-parser');
const WebSocket = require('ws');

const app = express();
const server = http.createServer(app);
const io = socketIo(server, {
  cors: {
    origin: "*",
    methods: ["GET", "POST"]
  }
});

// Middleware
app.use(cors());
app.use(bodyParser.json());
app.use(express.static('public'));

// Store system metrics
let systemMetrics = {
  cpu: 0,
  memory: 0,
  disk: 0,
  network: 0,
  processes: [],
  timestamp: new Date()
};

// Function to collect system metrics
async function collectSystemMetrics() {
  try {
    // CPU usage
    const cpuData = await si.currentLoad();
    systemMetrics.cpu = cpuData.currentLoad;

    // Memory usage
    const memData = await si.mem();
    systemMetrics.memory = (memData.used / memData.total) * 100;

    // Disk usage
    const diskData = await si.fsSize();
    const mainDisk = diskData.find(disk => disk.fs.includes('C:') || disk.fs.includes('/'));
    if (mainDisk) {
      systemMetrics.disk = mainDisk.use;
    }

    // Network stats
    const netData = await si.networkStats();
    if (netData && netData.length > 0) {
      systemMetrics.network = netData[0].rx_sec + netData[0].tx_sec; // Combined bytes per second
    }

    // Process list
    systemMetrics.processes = await si.processes();
    systemMetrics.timestamp = new Date();

    return systemMetrics;
  } catch (error) {
    console.error('Error collecting system metrics:', error);
    return systemMetrics;
  }
}

// WebSocket for real-time updates
io.on('connection', (socket) => {
  console.log('Client connected:', socket.id);

  // Send initial metrics
  socket.emit('systemMetrics', systemMetrics);

  // Send metrics every 2 seconds
  const interval = setInterval(async () => {
    await collectSystemMetrics();
    socket.emit('systemMetrics', systemMetrics);
  }, 2000);

  socket.on('disconnect', () => {
    console.log('Client disconnected:', socket.id);
    clearInterval(interval);
  });

  // Handle optimization requests
  socket.on('optimizeSystem', async (data) => {
    console.log('Optimization request received:', data);
    
    // Simulate optimization process
    const result = await performOptimization(data.type);
    socket.emit('optimizationComplete', result);
  });

  // Handle process management
  socket.on('manageProcess', async (data) => {
    console.log('Process management request:', data);
    
    const result = await manageProcess(data.action, data.pid);
    socket.emit('processManagementResult', result);
  });
});

// API Routes
app.get('/api/metrics', async (req, res) => {
  try {
    await collectSystemMetrics();
    res.json(systemMetrics);
  } catch (error) {
    res.status(500).json({ error: error.message });
  }
});

app.get('/api/cpu', async (req, res) => {
  try {
    const cpuData = await si.currentLoad();
    res.json(cpuData);
  } catch (error) {
    res.status(500).json({ error: error.message });
  }
});

app.get('/api/memory', async (req, res) => {
  try {
    const memData = await si.mem();
    res.json(memData);
  } catch (error) {
    res.status(500).json({ error: error.message });
  }
});

app.get('/api/processes', async (req, res) => {
  try {
    const processes = await si.processes();
    res.json(processes);
  } catch (error) {
    res.status(500).json({ error: error.message });
  }
});

app.post('/api/optimize', async (req, res) => {
  try {
    const { type } = req.body;
    const result = await performOptimization(type);
    res.json(result);
  } catch (error) {
    res.status(500).json({ error: error.message });
  }
});

app.post('/api/process', async (req, res) => {
  try {
    const { action, pid } = req.body;
    const result = await manageProcess(action, pid);
    res.json(result);
  } catch (error) {
    res.status(500).json({ error: error.message });
  }
});

// Performance optimization functions
async function performOptimization(type) {
  try {
    switch (type) {
      case 'cpu':
        // Optimize CPU-intensive processes
        const processes = await si.processes();
        const cpuIntensive = processes.list.filter(p => p.cpu > 50);
        
        for (const proc of cpuIntensive) {
          // In a real implementation, we would adjust process priority
          console.log(`Optimizing CPU for process: ${proc.name}, PID: ${proc.pid}`);
        }
        
        return { success: true, optimized: cpuIntensive.length, type: 'cpu' };
        
      case 'memory':
        // Memory optimization
        const memData = await si.mem();
        const memPercent = (memData.used / memData.total) * 100;
        
        if (memPercent > 80) {
          // In a real implementation, we would clean memory
          console.log('Performing memory optimization...');
        }
        
        return { success: true, type: 'memory', freed: memData.free };
        
      case 'disk':
        // Disk optimization
        console.log('Performing disk optimization...');
        return { success: true, type: 'disk' };
        
      case 'all':
        // Perform all optimizations
        const cpuResult = await performOptimization('cpu');
        const memResult = await performOptimization('memory');
        const diskResult = await performOptimization('disk');
        
        return {
          success: true,
          type: 'all',
          cpu: cpuResult,
          memory: memResult,
          disk: diskResult
        };
        
      default:
        return { success: false, error: 'Invalid optimization type' };
    }
  } catch (error) {
    console.error('Optimization error:', error);
    return { success: false, error: error.message };
  }
}

async function manageProcess(action, pid) {
  try {
    switch (action) {
      case 'kill':
        // In a real implementation, we would kill the process
        console.log(`Killing process with PID: ${pid}`);
        return { success: true, action: 'kill', pid: pid };
        
      case 'priority':
        // In a real implementation, we would change process priority
        console.log(`Changing priority for process with PID: ${pid}`);
        return { success: true, action: 'priority', pid: pid };
        
      case 'details':
        // Get process details
        const processes = await si.processes();
        const process = processes.list.find(p => p.pid === pid);
        return { success: true, action: 'details', process: process };
        
      default:
        return { success: false, error: 'Invalid action' };
    }
  } catch (error) {
    console.error('Process management error:', error);
    return { success: false, error: error.message };
  }
}

// Start server
const PORT = process.env.PORT || 3000;
server.listen(PORT, () => {
  console.log(`EchoBooster Node.js server running on port ${PORT}`);
  console.log(`WebSocket server running on port ${PORT}`);
  console.log(`API endpoints available at http://localhost:${PORT}/api`);
});

// Collect metrics immediately on startup
collectSystemMetrics();

// Handle graceful shutdown
process.on('SIGINT', () => {
  console.log('\nShutting down EchoBooster server...');
  server.close(() => {
    console.log('Server closed.');
    process.exit(0);
  });
});