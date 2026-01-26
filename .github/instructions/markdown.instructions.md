# Markdown Style Guide

## General Principles
- Use markdown for documentation, README files, and changelog entries.
- Prioritize clarity and readability over clever formatting.
- Keep line length reasonable (80-100 characters) for easier diffs and readability.

## Headings
- Use `#` for top-level headings, `##` for sections, `###` for subsections.
- Do not skip heading levels (e.g., don't jump from `#` to `###`).
- Use sentence case for headings.

## Blank Lines
- Insert **one blank line** between major sections (headings).
- Insert **one blank line** between paragraphs.
- Insert **one blank line** before and after code blocks.
- Insert **one blank line** before and after lists.
- Do not use multiple consecutive blank lines.

## Lists
- Use `-` for unordered lists; use `1.`, `2.`, etc. for ordered lists.
- Indent nested items with two spaces.
- Add blank line before list if preceded by text.

## Code and Code Blocks
- Use inline backticks `` ` `` for code references (e.g., `variable_name`, `function()`).
- Use triple backticks with language identifier for code blocks:
    ```csharp
    public class Example { }
    ```
- Always include blank line before and after code blocks.

## Links and URLs
- Use **markdown link syntax** for all URLs: `[display text](https://example.com)`
- **Never use bare URLs** (e.g., `https://example.com`); they reduce readability and break easily in formatting.
- Use descriptive link text that explains the destination (e.g., `[C# Development Guidelines](./csharp.instructions.md)`, not `[link](./csharp.instructions.md)`).
- For relative links within the repository, use relative paths: `[file](./path/to/file.md)`.

## Emphasis
- Use `**bold**` for strong emphasis (e.g., important terms, warnings).
- Use `*italic*` sparingly for contextual emphasis.
- Avoid overuse of emphasis.

## Formatting Best Practices
- Use `>` for blockquotes to highlight important notes or warnings.
- Use tables for structured data; ensure alignment is clear.
- Avoid HTML unless markdown cannot express the structure.

## Example Structure