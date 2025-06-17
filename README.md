# WpfKioskApp

This repository contains `WpfKioskApp`, a WPF (.NET Framework 4.7.2) kiosk application that embeds a WebView2 browser and relays messages to a TCP server.  The UI is served from local HTML/JavaScript assets.

## Projects

- `src/WpfKioskApp` – WPF application source code.
- `src/WpfKioskApp.sln` – Visual Studio solution file.

## Features

- WebView2 hosting of the local frontend under `UI/Assets`.
- Resilient asynchronous TCP client with automatic reconnection.
- Bidirectional message relay between the TCP connection and the JavaScript frontend using `WebMessageBridge`.
- Structured logging using log4net with rolling log files under `logs`.

## Building

Open `src/WpfKioskApp.sln` with Visual Studio 2019 or later and restore NuGet packages.

## TODO

- Add unit tests and further error handling.
