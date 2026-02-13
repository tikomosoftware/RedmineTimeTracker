# Redmine Time Tracker - User Guide

English | **[日本語](USER_GUIDE.md)**

---

**Version**: 1.0  
**Last Updated**: February 13, 2026

---

## 📖 Table of Contents

1. [Introduction](#introduction)
2. [Installation](#installation)
3. [Initial Setup](#initial-setup)
4. [Basic Usage](#basic-usage)
5. [Advanced Features](#advanced-features)
6. [Troubleshooting](#troubleshooting)
7. [FAQ](#faq)

---

## Introduction

### About This Application

**Redmine Time Tracker** is a desktop application designed to streamline time entry in Redmine.

**Primary Purpose**: To **save time** on daily time entry tasks

This application specializes in **creating and updating** time entries. For deletion or detailed editing, please use the Redmine web interface.

### Recommended For

- ✅ Users who enter time for the same tickets daily
- ✅ Users who find it tedious to batch-enter time at month-end
- ✅ Users who want to automate recurring tasks like meetings or standups
- ✅ Users who want to eliminate missed time entries

### What You Can Do

1. **Template Feature**: Register frequently used tickets as templates
2. **Bulk Registration**: Register an entire month's time entries with one click
3. **Calendar Display**: Visualize registration status
4. **Flexible Frequency Settings**: Support for daily, weekly, and monthly patterns

---

## Installation

### Requirements

- **OS**: Windows 10 or later
- **Network**: Connection to Redmine server
- **Redmine**: Version 3.0 or later (REST API support required)

### Package Selection

Choose from two package types:

#### 1. Standalone (Recommended)
- **Filename**: `RedmineSupTool-vX.X.X-standalone.zip`
- **Features**: Includes .NET Runtime
- **Benefits**: **No installation or setup required**. Extract and run immediately.
- **For**: Users who want the simplest experience

#### 2. Framework-dependent (Lightweight)
- **Filename**: `RedmineSupTool-vX.X.X-framework-dependent.zip`
- **Features**: Application only
- **Requirements**: Requires separate [.NET 9.0 Runtime](https://dotnet.microsoft.com/download/dotnet/9.0) installation
- **Benefits**: Very small file size
- **For**: Users who already have .NET Runtime installed

### Installation Steps

1. **Download the ZIP file**
   - Download your preferred package from the GitHub Releases page

2. **Extract the ZIP file**
   - Right-click the downloaded ZIP file → "Extract All"
   - Extract to any folder (e.g., `C:\Apps\RedmineSupTool`)

3. **Launch the application**
   - Double-click `redmineSupTool.exe` in the extracted folder

> **💡 Tip**: Create a desktop shortcut for easy access

---

## Initial Setup

### 1. First Launch Configuration

When you launch the application for the first time, the settings dialog will appear automatically.

### 2. Enter Redmine Connection Information

Enter the following information:

#### Redmine URL
- Enter your organization's Redmine server URL
- **Example**: `https://redmine.example.com`
- **Note**: Do not include a trailing `/`

#### API Key
- Enter your Redmine API Key
- How to obtain your API Key:
  1. Log in to Redmine
  2. Click your account name in the top right → "My account"
  3. Click "API access key" in the right menu
  4. Click "Show" and copy the displayed key

> **⚠️ Important**: The API Key is stored locally in plain text. Use caution on shared computers.

### 3. Connection Test

1. Click the "Test Connection" button
2. If "Connection successful!" appears, you're ready
3. If an error appears, verify your URL and API Key

### 4. Save Settings

Click the "Save" button to save your settings.

---

## Basic Usage

### Step 1: Create Work Templates

#### What is a Template?

A feature that saves frequently used ticket information (ticket ID, hours, activity, etc.).

#### Creating a Template

1. **Click the "+ Add" button**
   - Located in the "Work Templates" section at the top of the main screen

2. **Enter template information**

   **Name**
   - Enter an identifier for the template
   - Examples: "Daily Standup", "Design Work", "Weekly Meeting"

   **Ticket ID**
   - Enter the Redmine ticket number
   - Example: `1234`

   **Hours**
   - Select work hours
   - Available in 0.5-hour increments, up to 8.0 hours
   - Examples: `0.5` (30 minutes), `4.0` (4 hours)

   **Activity**
   - Select the Redmine activity
   - Examples: "Design", "Development", "Testing"
   - Note: Activities registered in Redmine are automatically displayed

   **Frequency**
   - Select when to apply this template
   - Details explained in the next section

3. **Click the "Save" button**

#### Enable/Disable Templates

You can enable or disable templates using the **checkbox at the left end** of each template row.

**Checked (☑)**:
- This template is **enabled**
- **Included** in bulk registration

**Unchecked (☐)**:
- This template is **disabled**
- **Excluded** from bulk registration (will not be registered)

**Use cases**:
- Temporarily disable templates you're not using
- Manage templates that are only used in specific months
- Keep templates without deleting them

> **💡 Tip**: If templates are not being registered, check the checkboxes first. Unchecked templates will not be registered.


#### Frequency Settings

Configure when the template should be applied.

##### 📅 Daily (Mon-Fri)
- Applied every weekday from Monday to Friday
- **Use cases**: Daily standups, daily reports

##### 📅 Weekly
- Applied on specified days of the week
- Multiple days can be selected
- **Use cases**: 
  - Monday weekly meetings
  - Tuesday and Thursday code reviews

**Configuration**:
1. Select "Weekly"
2. Check the target day checkboxes
   - Example: For Monday only, check "Mon"
   - Example: For Tue & Thu, check "Tue" and "Thu"

##### 📅 Monthly
- Applied once per month on a specified day
- **Use cases**: Month-end reports, monthly meetings

**Configuration**:
1. Select "Monthly"
2. Select the date
   - "1" to "31": Applied on that day (e.g., 5th of every month)
   - "End of Month": Applied on the last day of the month

> **💡 Tip**: If you select "31" for months without 31 days (February, April, etc.), it won't be applied that month

---

### Step 2: Check Registration Status on Calendar

#### Understanding the Calendar

The monthly calendar is displayed in the lower half of the main screen.

#### Date Cell Color Meanings

| Color | Meaning |
|-------|---------|
| **Green background** | Time entry registered |
| **White background** | Weekday, not registered |
| **Gray background** | Weekend (not a registration target) |
| **Light orange background** | Excluded date (holiday setting) |
| **Light blue background** | Work day (weekend set as work day) |

#### Date Cell Display Content

Each date cell displays the following information:

- **✓ 4.5h**: Time entry registered (total 4.5 hours)
- **🚫 Holiday**: Set as excluded date
- **🏢 Work Day**: Weekend set as work day
- **- Unregistered**: Weekday, not registered

#### Changing Target Month

Change the displayed month from the "Target Month" dropdown at the top of the screen.
- Previous month, current month, and next month are available

---

### Step 3: Execute Bulk Registration

#### What is Bulk Registration?

A feature that automatically registers an entire month's time entries to Redmine based on created templates.

#### Bulk Registration Steps

1. **Preview (Recommended)**
   - Click the "Preview" button
   - Verify which templates will be applied on which days
   - Close the window after verification

2. **Execute Bulk Registration**
   - Click the "Bulk Register for Month" button
   - A confirmation dialog will appear

3. **Verify in Confirmation Dialog**
   - The target month and number of entries are displayed
   - Click "Yes" if everything looks correct

4. **Registration Process Execution**
   - The registration process begins
   - Progress is displayed in the status bar at the bottom
   - Do not close the screen during processing

5. **Verify Results**
   - After completion, a results dialog appears
   - The following information is displayed:
     - **New**: Number of newly registered entries
     - **Overwrite**: Number of updated existing entries
     - **Skipped**: Number of days not registered (weekends, excluded dates, etc.)
     - **Errors**: Number of errors that occurred

#### Registration Target Days

Only days meeting the following conditions are registered:

✅ **Days that will be registered**
- Weekdays (Monday-Friday)
- Weekends set as work days

❌ **Days that will not be registered**
- Saturdays and Sundays (unless set as work days)
- Days set as excluded dates

#### Handling Existing Entries

- **If an entry with the same day and ticket ID already exists**: It will be overwritten
- **If multiple templates apply to the same day**: Each is registered as a separate entry

> **⚠️ Caution**: Bulk registration may overwrite existing entries. We recommend previewing beforehand.

---

### Step 4: Edit and Delete Templates

#### Editing Templates

1. Click the "Edit" button for the template you want to edit
2. Modify the template information
3. Click the "Save" button

#### Deleting Templates

1. Click the "Delete" button for the template you want to delete
2. Click "Yes" in the confirmation dialog

> **💡 Tip**: If you want to temporarily stop using a template without deleting it, uncheck the checkbox to disable it (see "Step 1: Create Work Templates" for details).


---

## Advanced Features

### Holiday and Work Day Settings

#### Setting Excluded Dates (Holidays)

Set weekdays as holidays to exclude them from bulk registration.

**How to set**:
1. Right-click the target date on the calendar
2. Select "Set as Holiday (Skip Registration)"

**Use cases**:
- Days you took paid leave
- Public holidays
- Company closure days

**How to remove**:
1. Right-click the excluded date
2. Select "Remove Exclusion"

#### Setting Work Days

Set weekends as work days to include them in bulk registration.

**How to set**:
1. Right-click a Saturday or Sunday on the calendar
2. Select "Set as Work Day"

**Use cases**:
- Days you worked on weekends
- Substitute work days

**How to remove**:
1. Right-click the work day
2. Select "Remove Work Day Setting"

---

### Using the Preview Feature

#### What is the Preview Feature?

A feature that lets you verify which templates will be applied on which days before executing bulk registration.

#### Understanding the Preview

Click the "Preview" button to see content like this:

```
== February 2026 Registration Preview ==

2/3 (Mon)
  Daily Standup (#1234) 0.5h
  Design Work (#5678) 4.0h

2/4 (Tue)
  Daily Standup (#1234) 0.5h
  Design Work (#5678) 4.0h

2/5 (Wed)
  Daily Standup (#1234) 0.5h
  Design Work (#5678) 4.0h
  Weekly Meeting (#9012) 1.0h

...

Total: 45 entries, 180.0h
```

#### How to Use the Preview

- ✅ Check for missing registrations
- ✅ Verify no unintended days are registered
- ✅ Confirm total hours are reasonable
- ✅ Check for template configuration errors

---

### Checking Communication Logs

#### What is the Communication Log?

A feature to view API requests made to Redmine by the application.

#### Displaying Logs

1. Click "View" in the menu bar
2. Check "Show Communication Log"
3. The log panel appears on the right side

#### Log Panel Operations

**Adjusting Width**:
- Drag the gray line between the main content and log panel to adjust width

**Clearing Logs**:
- Click the "Clear" button at the bottom of the log panel

#### Understanding Logs

Logs record information like this with timestamps:

```
[09:32:45] --- Data Load Start ---
[09:32:45] Templates loaded: 6
[09:32:45] GET /enumerations/time_entry_activities.json
[09:32:46] Activities loaded: 5
[09:32:46] GET /time_entries.json?spent_on=>=2026-02-01&spent_on<=2026-02-29
[09:32:47] Existing time entries loaded: 45
[09:32:47] --- Data Load Complete ---
```

#### Using Logs

- ✅ Investigating error causes
- ✅ Verifying which APIs are being called
- ✅ Confirming communication is working properly

---

### Changing Settings

#### Changing Connection Settings

To change your Redmine URL or API Key:

1. Click "Settings" in the menu bar
2. Select "Connection Settings..."
3. The settings dialog appears
4. Change the necessary information
5. Test the connection (recommended)
6. Click the "Save" button

---

## Troubleshooting

### Connection Errors

#### Error: "No connection could be made because the target machine actively refused it"

**Cause**:
- Redmine server is not running
- Not connected to network
- Incorrect Redmine URL

**Solution**:
1. Verify you can access the Redmine URL in a browser
2. Check network connection
3. Verify Redmine URL in settings
4. Check firewall or proxy settings

---

#### Error: "401 Unauthorized"

**Cause**:
- Incorrect API Key
- API Key has been disabled

**Solution**:
1. Log in to Redmine and re-verify your API Key
2. Re-enter the API Key in settings
3. Run a connection test

---

#### Error: "404 Not Found"

**Cause**:
- Incorrect Redmine URL
- Redmine REST API is disabled

**Solution**:
1. Verify the Redmine URL doesn't have a trailing `/` (not needed)
2. Ask your Redmine administrator if REST API is enabled

---

### Bulk Registration Issues

#### "No valid templates found" message

**Cause**:
- All templates are disabled

**Solution**:
1. Enable at least one template checkbox in the template list

---

#### Some days are not registered

**Cause**:
- Those days are set as excluded dates
- Template frequency settings are incorrect

**Solution**:
1. Check the target days on the calendar (orange border indicates excluded dates)
2. If excluded, right-click → "Remove Exclusion"
3. Use the preview feature to verify template application

---

#### Existing entries are being overwritten

**Cause**:
- Bulk registration overwrites entries with the same day and ticket ID

**Solution**:
- If you don't want to overwrite, disable the relevant template or set as excluded date
- We recommend using the preview feature beforehand

---

### Cannot Save Template

#### "Please enter a valid Ticket ID" message

**Cause**:
- Ticket ID contains non-numeric characters

**Solution**:
- Enter only numbers for Ticket ID (e.g., `1234`)

---

#### "Please select at least one day" message

**Cause**:
- Frequency set to "Weekly" but no days selected

**Solution**:
- Check at least one target day checkbox

---

### Calendar Not Displaying

**Cause**:
- Error occurred during data loading

**Solution**:
1. Display communication log to check error details
2. Click the "↻ Refresh" button to reload
3. If still not resolved, restart the application

---

## FAQ

### Q1: Where can I get my API Key?

**A**: After logging in to Redmine, follow these steps:
1. Click your account name in the top right → "My account"
2. Click "API access key" in the right menu
3. Click "Show" and copy the displayed key

---

### Q2: Is my API Key stored securely?

**A**: No, the API Key is stored locally in **plain text**.
- Storage location: `C:\Users\{Username}\AppData\Local\RedmineSupTool\settings.json`
- Use caution on shared computers

---

### Q3: Does it support multiple projects?

**A**: The current version doesn't support multiple projects.
- However, you can register time to tickets from different projects by specifying different ticket IDs

---

### Q4: What happens to existing entries when I execute bulk registration?

**A**: Entries with the same day and ticket ID will be **overwritten**.
- If you don't want to overwrite, disable the relevant template or set as excluded date

---

### Q5: What should I do if I worked on weekends?

**A**: Right-click the target weekend on the calendar → Select "Set as Work Day"
- Weekends set as work days are included in bulk registration

---

### Q6: Can holidays be automatically excluded?

**A**: The current version doesn't have automatic holiday detection.
- You need to manually set excluded dates
- Planned for future versions

---

### Q7: Can I register time in increments other than 0.5 hours?

**A**: The current version only supports 0.5-hour increments.
- Options: 0.5, 1.0, 1.5, 2.0, ..., 8.0

---

### Q8: How many templates can I create?

**A**: There is no limit. Create as many as you need.

---

### Q9: Where is data stored?

**A**: All data is stored locally:
- Storage location: `C:\Users\{Username}\AppData\Local\RedmineSupTool\`
- Stored data:
  - Connection settings (`settings.json`)
  - Templates (`templates.json`)
  - Excluded dates (`excluded_dates.json`)
  - Work days (`work_days.json`)

---

### Q10: How do I uninstall?

**A**: Follow these steps:
1. Delete the application folder
2. (Optional) To also delete settings:
   - Delete the `C:\Users\{Username}\AppData\Local\RedmineSupTool\` folder

---

### Q11: Where should I check when an error occurs?

**A**: Check in this order:
1. **Status Bar**: Error messages appear in the status bar at the top (red, bold)
2. **Communication Log**: Menu → View → Show Communication Log for detailed logs
3. **Error Dialog**: Important errors display in a dialog

---

### Q12: I want to use the same settings on multiple PCs

**A**: You can copy the settings files:
1. Copy settings from PC1:
   - `C:\Users\{Username}\AppData\Local\RedmineSupTool\`
2. Paste to the same location on PC2

> **⚠️ Caution**: The API Key is stored in plain text, so be mindful of security

---

### Q13: If I start using it mid-month, will it register for past dates?

**A**: Yes, bulk registration targets **all days** in the target month.
- Past dates will also be registered
- Set dates you don't want registered as excluded dates

---

### Q14: Can I register time for multiple tickets on the same day?

**A**: Yes, you can.
- Create multiple templates with different ticket IDs
- When multiple templates apply to the same day, each is registered as a separate entry

---

### Q15: Can I add comments?

**A**: The current version doesn't have a comment feature.
- Planned for future versions

---

### Q16: Can I delete registered time entries?

**A**: No, this application does not have a deletion feature.

**Reasons**:
- The primary purpose of this app is to **save time** on time entry tasks
- It specializes in **creating and updating** time entries
- The app cannot distinguish between entries it created and manually created entries
- To avoid the risk of accidental deletion

**How to delete time entries**:
- Please delete them from the Redmine web interface

**If you registered entries by mistake**:
1. Delete the incorrect entries from the Redmine web interface
2. Fix the template
3. Re-register using this app (will overwrite with correct content)

---

## Support & Contact

If your issue isn't resolved, please contact us with the following information:

1. **Error Message**: Error content displayed in status bar or dialog
2. **Communication Log**: Log content (screenshot or copy)
3. **Redmine Version**: Your Redmine version
4. **Steps**: Steps leading to the error

---

## License

© 2026 tikomo software

---

**End of User Guide**

We hope this guide helps you enjoy using Redmine Support Tool!
