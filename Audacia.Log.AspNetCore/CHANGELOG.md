# Changelog

## 4.0.1 - 2024-11-20
### Added
- No new functionality added

### Changed
- No functionality changed

### Fixed
- Updated vulnerable dependencies ([#8](https://github.com/audaciaconsulting/Audacia.Log/pull/8))

## 4.0.0 - 2024-10-07

### Added

- A new telemetry initialiser and Action filter for enrich response body from ASPNET API actions. (
  See `LogResponseBodyActionTelemetryInitialiser` and `LogResponseBodyActionFilterAttribute`)
- A new telemetry initialiser to enrich dependency telemetry with the request and response body if configured. (
  See `HttpDependencyBodyCaptureTelemetryInitializer`)

### Changed

- **Breaking:** Renamed `AddRequestBodyTelemetry` to `AddActionRequestBodyTelemetry` and setup from `service.AddRequestBodyTelemetry()` to `services.AddActionRequestBodyTelemetry()`
- Added configuration to enforce code analysis for `Audacia.Log`, `Audacia.Log.AspNetCore`
  and `Audacia.Log.AspNetCore.Tests`.
    - Fixed all violation from enforce static analysis.

## 3.0.0 - 2024-07-25

### Added

- No new functionality added

### Changed

- Split telemetry initialiser in to claims and request
  body ([4339ae3](https://github.com/audaciaconsulting/Audacia.Log/pull/1/commits/4339ae3a396061c256c00d82b7a2e0a90e1bd2d1))