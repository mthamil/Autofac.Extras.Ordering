# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]


## [4.0.0] - 2021-08-07

### Changed

- Upgraded to Autofac 6.
- Updated supported .NET Standard version to 2.0.


## [3.0.0] - 2018-07-27

### Changed

- Upgraded to .NET Standard. 


## [2.0.0] - 2014-06-09

### Fixed

- Fixed bug where registering only a subset of a type as ordered would crash.

### Added

- Added feature to allow optionally providing a starting index when registering multiple types as ordered.

### Removed

- Removed obsolete code: `OrderedEnumerableParameter`, `UsingOrdering()`, and `UseOrdering()`.


## [1.3.0] - 2014-05-27

### Added

- Added `OrderByRegistration()` method for component registrations that span multiple types (using `ScanningActivatorData`).
- Created `OrderedRegistrationSource`, replacing the now obsolete `UsingOrdering()`, `UseOrdering()`, and `OrderedEnumerableParameter`. These will be removed in a future version.


## [1.2.0] - 2014-05-26

### Added

- Enabled passing parameters to `ResolveOrdered()`.
- Added support for metadata in addition to ordering information.


## [1.1.0] - 2014-05-21

### Added

- Added a method to manually resolve ordered dependencies.