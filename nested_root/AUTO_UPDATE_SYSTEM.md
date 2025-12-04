# EchoBooster Auto-Update System

## Overview
The auto-update system for EchoBooster checks for updates from the GitHub repository at https://github.com/aidandelange170-hub/Echo and automatically downloads and installs updates in the background.

## How It Works

### 1. Startup Process
- On application startup, the system first checks for any cached updates from a previous session
- If cached updates exist, they are automatically installed
- The system then begins periodic background checking for new updates

### 2. Update Detection
- The system connects to the GitHub API to retrieve the latest commit information
- It fetches the complete file tree from the repository (checking both `main` and `master` branches)
- Each repository file's SHA hash is compared with the local file's SHA hash
- Files that don't exist locally or have different hashes are identified as updates

### 3. Background Scanning
- The system runs a background task that periodically checks for updates (every 30 minutes by default)
- When updates are found, they are downloaded and cached in a temporary location
- The user is notified that updates are ready to be applied

### 4. Update Installation
- Updates are cached in a temporary directory (`%TEMP%/EchoBoosterUpdates`)
- When the user chooses to update or on application restart, cached updates are installed
- The system copies updated files from the cache to their proper locations
- After installation, the application automatically restarts to apply the updates

### 5. File Comparison Method
- SHA-256 hashing is used to compare local files with repository files
- This ensures accurate detection of file changes
- Only files with different hashes or missing files are downloaded

## Key Features

### Automatic Background Updates
- Background scanning runs continuously without user interaction
- Updates are downloaded automatically when detected
- Updates are applied on the next application restart

### Safe Update Process
- Updates are cached before installation to prevent corruption
- The system verifies file hashes to ensure integrity
- Automatic restart ensures updates are properly applied

### Branch Compatibility
- The system checks both `main` and `master` branches for compatibility
- This ensures updates can be retrieved regardless of the default branch name

### Error Handling
- Comprehensive error handling prevents crashes during update operations
- User-friendly error messages are displayed when issues occur
- Background checks continue even if individual attempts fail

## Implementation Details

### UpdateManager Class
- Manages the entire update process
- Handles GitHub API communication
- Performs file comparison and download operations
- Manages the background update checking task

### File Management
- Uses SHA-256 hashing for accurate file comparison
- Preserves directory structure during update installation
- Creates necessary directories automatically during installation

### Background Task
- Runs continuously to check for updates
- Configurable check interval (default: 30 minutes)
- Continues running even if individual checks fail

## Usage
The auto-update system runs automatically:
1. On application startup, cached updates are installed
2. Background scanning checks for new updates every 30 minutes
3. When updates are available, they are downloaded in the background
4. Users are notified when updates are ready to apply
5. Updates are applied automatically on application restart