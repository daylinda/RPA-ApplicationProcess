# RPA-ApplicationProcess

RPA-ApplicationProcess is a multi-part project aimed at streamlining the repetitive parts of applying for jobs (upload resume/cover letter, answer common questions, submit, and track outcomes). The first target workflow is automating applications on **SEEK**, with room to expand to other platforms later. :contentReference[oaicite:1]{index=1}

> **Status:** Early-stage / evolving. Expect breaking changes.

---

## What’s in this repo?

This repository currently contains two main components: :contentReference[oaicite:2]{index=2}

### 1) `RPA-Api/`
A .NET REST API layer intended to support the automation pipeline (e.g., storing resumes, extracting structured data, saving Q&A, tracking application status, etc.). The repo topics indicate Firebase + .NET + REST API as core parts of the stack. :contentReference[oaicite:3]{index=3}

### 2) `job-tracker-extension/`
A browser extension (TypeScript-based) to support job tracking and/or capturing job details during the application process. :contentReference[oaicite:4]{index=4}

---

## High-level goal

Reduce the time/effort spent repeating the same steps across job applications by building an automation-friendly workflow:

1. Upload resume (and optionally cover letter).
2. Extract key profile details (skills, experience, eligibility, location preferences).
3. Find suitable roles (starting with SEEK).
4. Assist with application submission (attended automation: “review + confirm” before submitting).
5. Track submitted applications and outcomes.

---

## Tech stack (current direction)

Based on the repository structure and topics: :contentReference[oaicite:5]{index=5}

- **Backend:** .NET (REST API)
- **Data/Storage:** Firebase (e.g., Firestore / Storage)
- **Client-side:** Chrome extension (TypeScript)

---

## Getting started (developer setup)

> These steps are a practical starting point. Update them as the codebase stabilises.

### Prerequisites
- .NET SDK (recommended: latest LTS)
- Node.js + npm (for the extension)
- Google Firebase project (Firestore/Storage as needed)
- Google Chrome (for extension testing)

### 1) Clone the repo
```bash
git clone https://github.com/daylinda/RPA-ApplicationProcess.git
cd RPA-ApplicationProcess