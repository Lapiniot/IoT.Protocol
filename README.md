# IoT.Protocol

A modular .NET library for implementing and extending IoT device communication protocols. This project is part of the Lapiniot ecosystem and provides protocol abstractions, enumerators, and control endpoints for various IoT protocols.

## Features
- **Protocol Abstractions:** Interfaces and base classes for IoT device communication.
- **UPnP Support:** SSDP search, reply, and control endpoint implementations.
- **Lumi Protocol:** Support for Lumi (Xiaomi Gateway with local network mode enabled) device discovery and control.
- **Yeelight Protocol:** Yeelight device enumeration and control.
- **SOAP Protocol:** SOAP client and control endpoint for device management.
- **Extensible:** Easily add support for new IoT protocols.

## Project Structure
- `IoT.Protocol/` — Core abstractions, enumerators, and protocol interfaces.
- `IoT.Protocol.Upnp/` — UPnP protocol support (SSDP, control endpoints, services).
- `IoT.Protocol.Lumi/` — Lumi protocol support (enumeration, control, events).
- `IoT.Protocol.Yeelight/` — Yeelight protocol support (commands, enumeration).
- `IoT.Protocol.Soap/` — SOAP protocol support (client, envelope, control).
- `IoT.Protocol.Upnp.Tests/` — Unit tests for UPnP protocol components.

## License
This project is licensed under the MIT License.