# ✨ Changelog (`v2.8.4`)

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## Version Info

```text
This version -------- v2.8.4
Previous version ---- v2.1.1
Initial version ----- v1.9.10
Total commits ------- 15
```

## [v2.8.4] - 2026-02-06

### 🔄 Changed

- extend CD pipeline with enhanced bug bounty publication workflow

## [v2.8.3] - 2026-01-19

### ❌ Removed

- remove deprecated process status error codes

## [v2.8.2] - 2026-01-06

### 🔒 Security

- update random code generator to preserve requested entropy as defined by the input length.
- return the normalized URL-safe Base64 encoded format of the generated code.

## [v2.8.1] - 2025-12-19

### 🔄 Changed

- Replaced custom alphanumeric code generation with a URL-safe Base64 code generator for email verification.

### 🔒 Security

- Increase entropy for verification codes.

## [v2.8.0] - 2025-11-27

### 🆕 Added

- add rate limiting for email changes

## [v2.7.1] - 2025-11-26

### 🆕 Added

- add email logs and diagnostics

## [v2.7.0] - 2025-11-25

### 🔄 Changed

- include email in status response and improve sent emails

## [v2.6.1] - 2025-11-25

### 🔄 Changed

- use alphanumeric chars for email verification code

## [v2.6.0] - 2025-10-15

### 🔄 Changed

- change email endpoint

## [v2.5.0] - 2025-09-22

### 🆕 Added

- configurable connector message type and tests for workers

## [v2.4.0] - 2025-09-19

### 🆕 Added

- make document template key configurable

## [v2.3.0] - 2025-09-19

### 🆕 Added

- add configurable email verification

## [v2.1.1] - 2025-01-24

### ❌ Removed

- removed TotalNumOfVoters setting

## [v2.1.0] - 2025-01-13

### 🔄 Changed

- consider live e-voting limits

## [v2.0.11] - 2024-11-25

### 🔄 Changed

- optimize SourceLink integration and use new ci/cd versioning capabilities
- prevent duplicated commit ids in product version, only use SourceLink plugin.
- extend .dockerignore file with additional exclusions

## [v2.0.10] - 2024-10-10

### 🔄 Changed

- apply post office box text for the address if street is not set

## [v2.0.9] - 2024-09-03

### 🔄 Changed

- migrate from gcr to harbor

## [v2.0.8] - 2024-08-27

### 🔄 Changed

- update bug bounty template reference
- patch ci-cd template version, align with new defaults

## [v2.0.7] - 2024-07-15

### 🔒 Security

- upgrade npgsql to fix vulnerability CVE-2024-0057

## [v2.0.6] - 2024-07-04

### 🔄 Changed

- update voting library to implement case-insensitivity for headers as per RFC-2616

## [v2.0.5] - 2024-06-03

### 🔄 Changed

- update link to code of conduct

## [v2.0.4] - 2024-05-14

### 🔄 Changed

- configure certificate pinning
- use secure https communication for CONNECT document delivery

## [v2.0.3] - 2024-03-13

### :lock: Security

- dependency and runtime patch policy
- use latest dotnet runtime v8.0.3

## [v2.0.2] - 2024-02-12

### 🆕 Added

- add dependency icu-libs to add support for globalization

## [v2.0.1] - 2024-02-12

### 🔄 Changed

- update swagger generator

## [v2.0.0] - 2024-02-12

BREAKING CHANGE: Updated service to .NET 8 LTS.

### :arrows_counterclockwise: Changed

- update to dotnet 8

### :lock: Security

- apply patch policy

## [v1.9.10] - 2024-01-31

### 🎉 Initial release for Bug Bounty
