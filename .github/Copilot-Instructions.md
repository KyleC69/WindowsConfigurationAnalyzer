**Github Copilot-Instructions**

**APPLICATION**: **Windows Configuration Analyzer**

**Description:** This application is intended to be a forensics grade Operating System analyzer for Windows. It is designed to analyze all areas of the operating system. It will use Microsoft's Rules Engine, heuristics, baseline caparitors and include AI powered analysis.

**DOCUMENTATION**: Documentation is located on the solution root on the docs folder. It contains architecture overviews, design decisions, and usage instructions. It is a work in progress and it is subject to change as the project evolves.

**Solution Structure**:
  - DataProbe project contains the data collection logic.
  - Root : Shared project for collection of miscellaneous files not tied to a project
  - RulesEngineStore - Contains rules for the RulesEngine library.
  - RuleAnalyzer - Main library project for the RulesEngine library and analyzer logic.
  - UserInterface - Contains User Interface, WinuUI 3 based .net 9.0 project.
  - UserInterface.Core/ - Contains core logic for the User Interface. 
  - Tests/ - Unit and integration tests for the analyzer library and UI components.

**Guardrails - Design Conventions**: *ENFORCED*
- All projects will be WinUI 3 and target .Net 9 There is a directory.build.props at the root to maintain uniformity in the projects AI DO NOT ALTER THIS FILE 
- Follow best practices for any new code you write, including SOLID principles, design patterns, and coding standards.
- User interface projects (UserInterface and UserInterface.Core) were created with TemplateStudio and must follow its conventions. Do not break existing patterns established by TemplateStudio.
- DO NOT remove or alter any existing documentation files unless directed to do so.
- DO NOT remove or alter any exiting comments or XML documentation in the codebase unless directed to do so. 
- Do not apply any formatting or style changes  It will be done automatically.

**AI Guardrails - ENFORCED**
- AI must adhere to the established design conventions and guardrails.
- AI must not alter any existing code or documentation without explicit permission.
- Keep all code changes isolated to the specific task or feature being implemented.
- Keep focused on the requested task and avoid unnecessary changes or additions.
- You are encouraged to make suggestions for improvements for any existing or new code.
- When suggesting changes, provide clear justifications and ensure they align with the project's goals and design principles.
- If a request is ambiguous, or against the documented design conventions, seek clarification before proceeding.
