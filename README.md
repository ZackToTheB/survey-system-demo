# Survey System Demo (Simplified)

This repository contains simplified code and schema examples based on a bespoke survey system I developed from scratch in my current role as a Senior Software Developer at a UK college.

## Purpose

This demo was created to support a technical interview and demonstrate the structure, logic, and design considerations behind a full-featured survey platform.

## Tech Stack

- **Backend:** .NET Framework (C#)
- **Frontend:** JavaScript
- **Database:** SQL Server
- **ETL:** SSIS (for syncing external submissions)
- **Visualisation:** Power BI

## Background

This system was developed to replace a limited third-party tool (Curriculum Surveyor), which didn't integrate well with internal reporting. I worked directly with stakeholders to identify requirements, design the structure, and deliver a fully integrated solution that feeds directly into our Power BI dashboards.


## Summary

- Supports both internal (authenticated) and external (public) surveys
- Handles named responders and is future-proofed for anonymous submissions (SHA256 hashing)
- Internal response data flows: internal form → internal DB → Power BI
- External response data flows: public form → external DB → SSIS → internal DB → Power BI
- Built using: .NET Framework (C#), JavaScript, SQL Server

## Contents

- `*.cs`: core logic snippets (question creation, form rendering)
- `SaveQuestions.js`: simplified front-end logic for collecting survey structure
- `erd.png`: database entity relationship diagram

## Process Overview

1. Survey templates are created via the admin interface
2. Each "occurrence" (instance of a survey) is versioned and linked to an audience
3. Users submit responses through internal (authenticated) or external (public) forms
4. Responses are saved directly (internally) or routed via external DB + SSIS import (public)
5. Results are viewed through Power BI dashboards

### Anonymous support (current and future)

The system includes support for anonymous submissions using SHA256 hashing.  
- Currently: external surveys **optionally** use audience IDs via query string, or submit anonymously (hash only)  
- Internally: all responses are linked directly to known audience members  
- Future updates may extend full anonymous handling with delayed inserts via the `responders` table

## Note

This repo contains only simplified, redacted code for demonstration purposes.  

---
