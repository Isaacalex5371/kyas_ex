# API Versioning Policy

## Purpose

This document defines how the Training Management System (TMS) API is versioned to ensure that existing client applications continue to work while new features are introduced. Our goal is to provide a stable API contract and allow clients enough time to migrate to newer versions.

---

# Versioning Strategy

The TMS API uses **URL Segment Versioning**.

Example:

GET /api/v1/courses

GET /api/v2/courses

Each API version has its own controller and contract.

---

# Breaking Changes

A new API version is required whenever one of the following changes is made:

- Removing an existing field from a response.
- Renaming an existing field.
- Changing the meaning or data type of a field.
- Changing HTTP status codes returned by an endpoint.
- Making an optional request field required.
- Tightening validation rules.
- Changing the default sorting or filtering behavior.
- Removing an endpoint.

These changes can break existing applications and therefore require a new API version.

---

# Non-Breaking (Additive) Changes

The following changes do NOT require a new API version:

- Adding a new optional response field.
- Adding a new endpoint.
- Adding optional query parameters.
- Improving performance.
- Internal code refactoring.
- Bug fixes that do not change the API contract.
- Adding optional request fields.

These changes are considered backward compatible.

---

# Deprecation and Sunset Policy

When a new API version is released:

- The previous version is marked as deprecated.
- Deprecation information is sent using HTTP response headers.
- The old version will remain available for at least **6 months**.
- Clients are encouraged to migrate before the sunset date.
- After the sunset date, the deprecated version may be removed.

---

# Communication Strategy

When a version is deprecated, the following communication methods are used:

- Deprecation HTTP header
- Sunset HTTP header
- Link header pointing to the new version
- CHANGELOG updates
- Email notifications to API consumers
- Documentation updates
- Release notes

---

# Client Migration

Clients are allowed to migrate directly to the latest API version.

Example:

Version 1 → Version 3

Clients are **not required** to upgrade through every intermediate version.

---

# Version Support

The API may support multiple versions at the same time.

Example:

- v1
- v2

Each version remains independent and maintains its own contract.

---

# Goals

The API versioning policy aims to:

- Prevent breaking existing applications.
- Allow safe introduction of new features.
- Provide clear migration guidance.
- Maintain a stable and reliable API.
- Improve long-term maintainability of the system.

---

# Summary

This policy ensures that the TMS API evolves without disrupting existing clients. Breaking changes always result in a new API version, while backward-compatible improvements are delivered within the current version whenever possible.