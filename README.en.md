# Redmine Time Tracker

English | **[日本語](README.md)**

---

**Make Redmine time entry easier!**

Redmine Time Tracker is a desktop application designed to streamline and automate your daily time entry tasks in Redmine.
Say goodbye to the tedious routine of "finding ticket IDs, calculating hours, and entering data repeatedly."

## 💭 Development Background

Previously, I used Python scripts to automate time entry from the command line.
However, in practice, there were several challenges:

- ❌ Registration status was not visible at a glance
- ❌ Checking which days were registered was tedious
- ❌ Command-line interface was not intuitive

So, I developed a GUI application with **calendar display** and **visual operations**.

**Results**:
- ✅ Monthly registration status visible at a glance
- ✅ Easy mouse-based operations
- ✅ Quickly identify missing entries

The evolution from command-line tool to GUI application has made time entry even more convenient.

## ✨ Features

### 📅 Monthly Calendar View
View your entire month's work records in a calendar format at a glance.
Easily see "which day," "which ticket," and "how many hours" you worked.
Checking for missing entries is simple.

### ⚡ Bulk Template Registration
Templatize recurring daily and weekly tasks.
Register routine tasks like "daily standup" or "weekly meetings" across the entire month with a single click.
No more repeating the same entries every day.

### 🎨 Modern and User-Friendly UI
A simple and beautiful design incorporating the latest Windows trends.
Intuitive operation that you can use immediately without a manual.

### 🔍 Transparent Communication Log
Built-in log viewer to check API requests made to Redmine.
You can see "what was sent," ensuring peace of mind.

## 📦 Package Options

Two packages are available to suit your needs. We recommend the **Standalone** version for most users.

### 1. Standalone (Recommended)
- **Filename**: `RedmineSupTool-vX.X.X-standalone.zip`
- **Features**: Includes .NET Runtime bundled
- **Benefits**: **No installation or setup required**. Extract and run immediately.

### 2. Framework-dependent (Lightweight)
- **Filename**: `RedmineSupTool-vX.X.X-framework-dependent.zip`
- **Features**: Application only package
- **Requirements**: Requires separate installation of [.NET 9.0 Runtime](https://dotnet.microsoft.com/download/dotnet/9.0)
- **Benefits**: Very lightweight file size

## 📋 System Requirements

- **Redmine**: Version 3.0 or later (REST API must be enabled)
- **OS**: Windows 10 or later

## 🚀 How to Use

1. Launch `redmineSupTool.exe`
2. On first launch, configure your Redmine **URL** and **API Key**
3. Run a connection test to ensure everything works
4. Create templates or register work time from the calendar

For detailed instructions, please refer to [USER_GUIDE.en.md](USER_GUIDE.en.md).

## 💻 Development Information

- **Language**: C# 13 / .NET 9.0
- **UI Framework**: WPF
- **Design**: Modern UI with Fluent Design principles

## 📜 Change Log

### v1.1.0 (2026/02/14)
- **✨ Enhanced Bulk Create Child Issues**
  - Added support for automatic hierarchical structure (parent-child) using indentation.
  - Added a common description/keyword field for all created issues.
  - Optimized sequential processing to ensure correct ID ordering in Redmine.
- **🐞 Bug Fixes & Stability**
  - Fixed an issue where the total time on the calendar was displayed incorrectly (including historical entries).
  - Fixed null reference warnings in resource retrieval.
  - Minor UI adjustments for the bulk creation dialog.

### v1.0.0 (2026/02/13)
- Initial release.

---

© 2026 tikomo software
