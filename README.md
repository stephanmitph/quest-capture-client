# Quest Capture Client
The Quest Capture Client Unity App enables you to capture your device's camera feed along with various tracking data and store it on a remote server.   

**Companion Server:** Setup the [Quest Capture Server](https://github.com/stephanmitph/quest-capture-server). Follow the instructions to get your own instance running. Configure this server in the settings of the client app.

**⚠️ Development Status:** This client is actively developed and manually tested.

## Features

### Core Capabilities
- **Device Support**: Compatible with Meta Quest 3
- **Real-time Data Capture**: Stream camera feed and tracking data
- **Custom Collections**: Create guided data collection sessions with custom prompts
- **Web Dashboard**: User interface for data management and visualization
- **Self-Hosted**: Complete control over your data and infrastructure

### Tracking Data (Per Frame)
- **Head Tracking**: Position and rotation
- **Hand Tracking**: Individual finger bone positions and rotations
- **Controller Tracking**: Position, rotation, and velocity data

## Planned features
- [ ] Include depth data 
- [ ] Ability to include audio in recordings

## Demo

https://github.com/user-attachments/assets/00d62809-4a3a-4eda-8820-71a6f5b2d977

## System Overview 

![systemoverview](https://github.com/user-attachments/assets/951117ed-d48f-489d-895a-52804657cfa8)

## Installation

### Prerequisites
- Meta Quest 3 headset
- Developer mode enabled on your Quest 3
- [Quest Capture Server](https://github.com/stephanmitph/quest-capture-server) running on your network

### Enable Developer Mode
1. Install the **Meta Quest Developer Hub** on your computer
2. Create a Meta developer account if you don't have one
3. Connect your Quest 3 to your computer via USB-C cable
4. In the Meta Quest Developer Hub, enable **Developer Mode** for your device
5. On your Quest 3, go to **Settings > System > Developer** and enable **USB Connection Dialog**

### Install APK from Releases
1. Download the latest APK file from the [Releases](https://github.com/stephanmitph/quest-capture-client/releases) page
2. Connect your Quest 3 to your computer via USB-C cable
3. Put on your headset and allow USB debugging when prompted
4. Install using one of these methods:

#### Method 1: Using Meta Quest Developer Hub
1. Open Meta Quest Developer Hub
2. Select your connected Quest 3 device
3. Click **Add Build** and select the downloaded APK file
4. Click **Install** to deploy to your headset

#### Method 2: Using SideQuest
1. Install [SideQuest](https://sidequestvr.com/) on your computer
2. Connect your Quest 3 and enable developer mode in SideQuest
3. Drag and drop the APK file onto the SideQuest interface
4. Follow the installation prompts

### Launch the App
1. In your Quest 3 headset, go to **Library > Unknown Sources**
2. Find and launch **Quest Capture**
3. Grant camera permissions when prompted

## Configuration

### First Time Setup
1. **Camera Permissions**: When launching for the first time, grant camera access permissions
2. **Spatial Data**: Enable spatial data permissions in Quest settings
3. **Network Configuration**: Configure your server connection in the app settings

### Settings Configuration

<details>
<summary><strong>Network Settings</strong></summary>

- **Server IP**: Enter the IP address of your Quest Capture Server
  - Find your server's IP with `ipconfig` (Windows) or `ifconfig` (Mac/Linux)
  - Example: `192.168.1.100`
- **Server Port**: Default is `8080` (must match your server configuration)
- **Connection Status**: Green indicator on main menu shows successful server connection

</details>

<details>
<summary><strong>Camera Settings</strong></summary>

- **Camera Eye**: Choose between Left or Right camera
  - **Left**: Uses left passthrough camera
  - **Right**: Uses right passthrough camera
- **Image Resolution**: Select capture resolution
  - **1280x960**: High quality (larger file sizes)
  - **800x600**: Medium quality (recommended)
  - **640x480**: Lower quality (smaller file sizes)
  - **320x240**: Lowest quality (fastest processing)

</details>

<details>
<summary><strong>Graphics Settings</strong></summary>

- **Image Quality**: JPEG compression quality
  - **100%**: No compression (largest files)
  - **75%**: High quality (recommended)
  - **50%**: Medium quality
  - **25%**: Low quality (smallest files)

</details>

<details>
<summary><strong>Collection Settings</strong></summary>

- **Custom Collections**: Create guided recording sessions
  - Set recording duration (5-120 seconds)
  - Add custom prompts for users
  - Organize recordings by collection ID

</details>

### Testing Your Setup
1. Start your Quest Capture Server
2. Launch Quest Capture on your headset
3. Go to **Settings** and verify green connection status
4. Create a test recording from the main menu
5. Check your server dashboard for received data

### Troubleshooting
- **Red connection status**: Check server IP/port and ensure server is running
- **Camera not working**: Verify camera permissions in Quest settings
- **Poor performance**: Lower image resolution and quality settings
- **App not visible**: Check **Unknown Sources** in your Quest **library**
### Troubleshooting
- **Red connection status**: Check server IP/port and ensure server is running
- **Camera not working**: Verify camera permissions in Quest settings
- **Performance**: Lower image resolution and quality settings
- **App not visible**: Check **Unknown Sources** in your Quest **library**

## Contributing

Contributions are welcome. If you have suggestions for improvements or new features, please open an issue or submit a pull request.
