# WpfKioskApp

This repository contains a sample WPF (.NET Framework 4.7.2) application that hosts a WebView2 control and communicates with a TCP server. The UI is provided by a local HTML/JavaScript page.

## Projects

- `src/WpfKioskApp` – WPF application source code.
- `src/WpfKioskApp.sln` – Visual Studio solution file.

## Features

- WebView2 hosting of `index.html`.
- Persistent TCP client with reconnect logic.
- Message relay between the TCP connection and the JavaScript frontend.
- Logging using log4net with rolling log files.

## Building

Open `src/WpfKioskApp.sln` with Visual Studio 2019 or later and restore NuGet packages.

## TODO

- Improve UI and connection status display.
- Add tests and further error handling.
