# Security Policy

## Scope

This project is a portfolio-focused hotel management system. It includes security-oriented application controls and automated tests, but it is not a substitute for production infrastructure hardening, monitoring, backups, key management, or an incident-response program.

## Reporting a Vulnerability

Do not post exploit details, credentials, tokens, or sensitive customer-style data in a public issue.

Use GitHub's private security reporting / security advisory flow when available. Otherwise, contact the repository owner through GitHub before publishing technical details.

A useful report includes:
- affected endpoint or component;
- reproduction steps;
- expected and actual behavior;
- impact;
- a minimal proof of concept with secrets removed.

## Production Safety

- Real JWT secrets and database credentials must come from deployment secret storage or environment configuration.
- Automatic migrations and demo-user seeding are disabled outside Development/Testing.
- Production migrations must be run as a controlled deployment step.
- Production startup requires explicit CORS origins.
- Development demo passwords must never be reused for real users.
- HTTPS, secure secret storage, database backups, monitoring, and log retention must be configured by the deployment environment.

## Automated Evidence

GitHub Actions validates backend build/tests and frontend lint/tests/build. The backend suite includes authorization, ownership isolation, validation, business-rule, API, and integration coverage.

## Supported Versions

Only the current `main` branch is maintained for this portfolio repository.
