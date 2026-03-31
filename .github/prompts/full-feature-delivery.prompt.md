---
description: "Plan, implement, and validate a complete feature end-to-end. Orchestrates all three agents (Planning → Coding → Testing) in sequence."
name: "Full Feature Delivery"
agent: "Orchestrator Agent"
argument-hint: "Feature title, description, and acceptance criteria"
---

# Complete Feature Development Workflow

Use this to deliver a feature from requirements → implementation → validation with all three agents working together.

## Provide

**Feature Title**: What are you building?

**Description**: What's the detailed requirement?

**Acceptance Criteria**: What must be true for this to be complete?

---

## The Orchestrator Will

1. **Ask Planning Agent** to analyze the codebase and create a step-by-step implementation plan
2. **Ask Coder Agent** to implement exactly what was planned
3. **Ask Testing Agent** to validate all acceptance criteria are met
4. **Loop back** if testing finds issues (Coder fixes → Testing re-validates)
5. **Report** final status and deliverables

## Example Format

```
Feature Title: Add User Profile API Endpoint

Description: Create a new REST API endpoint that allows authenticated users to view and update their profile information. The endpoint should support GET and PUT methods.

Acceptance Criteria:
- GET /api/users/me returns current user's profile (name, email, avatar)
- PUT /api/users/me accepts profile updates (name, avatar)
- Unauthorized requests (no token) return 401
- Invalid data returns 400 with validation errors
- Changes are persisted to database
- All endpoints have unit and integration tests
```

**Start by describing your feature above, and the Orchestrator will handle the rest!**
