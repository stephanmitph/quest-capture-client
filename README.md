# Quest Capture Client
The Quest Capture Client Unity App enables you to capture your device's camera feed along with various tracking data, and store it on a remote server.   

The Quest Capture server can be found here: [Quest Capture Server](https://github.com/stephanmitph/quest-capture-server). Follow the instructions to get your own instance running. Configure this server in the settings of the client app.

This client is still in development and manually tested. If you encounter any issues or bugs, please feel free to contribute!

## Features
- Capture raw image and tracking data from Quest (3 and 3S) devices
- Web interface for content visualization
- Docker support for easy deployment for self-hosting
- Tracking data per frame
  - Head position / rotation
  - Hand position / rotation & Individual Bones
  - Controller position / rotation / velocity

## Demo

<video width="640" height="auto" controls>
  <source src=".github/Demo.mp4" type="video/mp4">
</video>

## Contributing

Contributions are welcome. If you have suggestions for improvements or new features, please open an issue or submit a pull request.

## License

This project is licensed under the MIT License.