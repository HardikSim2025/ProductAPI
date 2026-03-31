---
name: "Multi-Agent Development Team"
description: "A structured team of specialized agents for planning, coding, and testing. Use when implementing features or fixes that require full development workflow."
---

# Multi-Agent Development Workflow

This workspace is configured with a three-agent development team that mirrors a real software development process. Each agent has a specialized role and limited, focused tools.

## The Team

### � [Orchestrator Agent](./.github/agents/orchestrator.agent.md) — **START HERE**
**Role**: Workflow coordination  
**Trigger**: When you have a complete feature/requirement and want full end-to-end development  
**Tools**: `agent`, `todo`
- Accepts user stories (title, description, acceptance criteria)
- Delegates to Planning Agent for analysis
- Delegates to Coder Agent for implementation
- Delegates to Testing Agent for validation
- Manages feedback loops if issues are found
- Provides executive summary of the entire workflow
- **This is your single entry point for complete features**

---

### �🎯 [Planning Agent](./.github/agents/planner.agent.md)
**Role**: Requirements analysis and planning  
**Trigger**: When you need to break down a feature or bugfix into actionable steps  
**Tools**: `read`, `search`, `todo`
- Analyzes user stories and acceptance criteria
- Examines the codebase to understand context
- Identifies required changes and gaps
- Creates step-by-step implementation plans
- Tracks work items

### 💻 [Coder Agent](./.github/agents/coder.agent.md)
**Role**: Code implementation  
**Trigger**: When you have a plan and need to implement it  
**Tools**: `read`, `edit`, `search`, `agent`
- Reads the implementation plan
- Makes targeted code changes
- Follows existing code patterns and conventions
- Writes or updates tests
- Hands off to Testing Agent

### ✅ [Testing Agent](./.github/agents/tester.agent.md)
**Role**: Quality validation  
**Trigger**: When you need to verify implementation meets requirements  
**Tools**: `read`, `search`, `execute`
- Runs test suites
- Verifies acceptance criteria
- Tests edge cases and scenarios
- Reports quality assessment
- Requests fixes if needed

## Workflow

**Pattern 1: Full Feature Implementation (Recommended)**
```
                    You (provide requirement)
                              ↓
                    Orchestrator Agent
                    /          |          \
            Planning -----> Coding -----> Testing
                    (analyze) (implement) (validate)
                              ↓
                    Ready for deployment
```

**Pattern 2: Direct Planning Only**
```
Planning Agent → [Plan only, no implementation]
```

**Pattern 3: Code Review / Testing Only**
```
Testing Agent → [Validate without full workflow]
```

**Pattern 4: Iterative Fixes**
```
Testing Agent → [Issues found]
       ↓
Coder Agent → [Fix issues]
       ↓
Testing Agent → [Re-validate]
```

## Example Usage

### 🎼 Using the Orchestrator (Recommended for full features)

Simply describe your feature and let the Orchestrator coordinate all three agents:

```
"/Full Feature Delivery"

Feature: Add email notifications for user actions
Description: When users perform key actions (signup, login, purchase), 
  send them email notifications. Use an email service like SendGrid.
Acceptance Criteria:
  - Signup sends welcome email
  - Purchase sends receipt email
  - All emails include unsubscribe link
  - Failed emails are logged
  - Notification preferences are respected
```

The Orchestrator will:
- Ask Planning Agent to design the implementation
- Ask Coder Agent to build it
- Ask Testing Agent to validate everything works

### 📌 Using individual agents (for specific tasks)

Ask the Planning Agent:
> "@Planning Agent: Create an implementation plan for [user story]"

Ask the Coder Agent:
> "@Coder Agent: Implement the plan from Planning Agent"

Ask the Testing Agent:
> "@Testing Agent: Validate the implementation against the acceptance criteria"

## Key Principles

- **Single Responsibility**: Each agent has one focused role
- **Tool Restriction**: Agents only access tools needed for their role
- **Clear Handoffs**: Agents communicate context explicitly for the next stage
- **Feedback Loops**: Testing Agent can request fixes from Coder Agent if issues found

## Benefits

✨ **Focused expertise** — Each agent specializes in one part of the workflow  
✨ **Reduced context confusion** — Limited tools prevent distraction  
✨ **Traceable work** — Clear transitions between planning, coding, and testing  
✨ **Team collaboration** — Agents can invoke each other via handoffs  

---

Start with the **Orchestrator Agent** when you have a new feature or requirement!

**How to use:**
1. In VS Code Chat, type `/` and select **"Full Feature Delivery"** (or type `@Orchestrator Agent`)
2. Provide your feature title, description, and acceptance criteria
3. Let it handle the entire workflow automatically
