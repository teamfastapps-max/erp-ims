# ERP-IMS — Institute Management System
## Complete System Architecture & Data Relationship Document

> **Last Updated:** 30 August 2026  
> **Version:** 1.0  
> **Purpose:** Single-source reference for understanding how every module, table, service, and view in the ERP-IMS system connects together.

---

## Table of Contents

1. [Project Overview](#1-project-overview)
2. [Architecture Layers](#2-architecture-layers)
3. [Database Schema — All Tables](#3-database-schema--all-tables)
4. [Entity Relationship Diagram](#4-entity-relationship-diagram)
5. [Module Breakdown](#5-module-breakdown)
6. [Stored Procedures Map](#6-stored-procedures-map)
7. [Data Flow: How Modules Connect](#7-data-flow-how-modules-connect)
8. [Key Patterns](#8-key-patterns)
9. [Implementation Status](#9-implementation-status)

---

## 1. Project Overview

**ERP-IMS** is a multi-tenant Institute Management System built with:

| Layer | Technology |
|-------|-----------|
| Presentation | ASP.NET Core 8.0 MVC, Razor Views, jQuery |
| Business Logic | C# Services (no EF — raw ADO.NET) |
| Data Access | SQL Server Stored Procedures via `DBHelper` |
| Authentication | Keycloak (OpenID Connect) |
| Session | Redis (user profile + permissions cache) |
| Primary Keys | `UNIQUEIDENTIFIER` (GUID) throughout |

**Multi-tenancy:** Every table has a `*_TenantId` column. All queries filter by tenant. The tenant ID comes from the Keycloak JWT `tenant_id` claim.

---

## 2. Architecture Layers

```
┌─────────────────────────────────────────────────────────────┐
│                     IMS.Web (Presentation)                   │
│  Controllers │ Razor Views │ JavaScript │ DI Extensions     │
├─────────────────────────────────────────────────────────────┤
│                    IMS.Services (Business Logic)             │
│  StudentService │ BatchService │ MasterService │ etc.       │
├─────────────────────────────────────────────────────────────┤
│                      IMS.DAL (Data Access)                  │
│  StudentDAL │ BatchDAL │ MasterDAL │ DBHelper               │
├─────────────────────────────────────────────────────────────┤
│                     IMS.Models (Shared)                      │
│  Entities │ ViewModels │ MasterConfigRegistry │ Dropdowns   │
├─────────────────────────────────────────────────────────────┤
│                     IMS.Helpers (Utilities)                  │
│  HardcodedMasterData │ Constants │ Options                   │
├─────────────────────────────────────────────────────────────┤
│                      SQL Server Database                     │
│  58 Tables │ 30+ Stored Procedures │ FK Constraints         │
└─────────────────────────────────────────────────────────────┘
```

**Request flow:**
```
Browser → Controller (extracts tenant_id from JWT)
       → Service (business validation, dropdown population)
       → DAL (builds SQL parameters)
       → DBHelper (executes stored procedure)
       → SQL Server → Returns data up the chain
       → Controller returns JSON { success, message, id }
```

---

## 3. Database Schema — All Tables

### 3.1 Complete Table List (58 Tables)

#### Category A: Organization & Tenancy

| Table | Suffix | PK | TenantId | Soft Delete |
|-------|--------|----|----------|-------------|
| `Organizations_O` | O | `O_Id` | *IS the tenant* | `O_DeletedAt` |
| `OrganizationSettings_OS` | OS | `OS_Id` | `OS_TenantId` | None |
| `Branches_B` | B | `B_Id` | `B_TenantId` | `B_DeletedAt` + `B_Status` |
| `BankMaster_BM` | BM | `BM_Id` (INT IDENTITY) | `BM_TenantId` | `BM_IsActive` |

#### Category B: Academic Structure

| Table | Suffix | PK | TenantId | Soft Delete |
|-------|--------|----|----------|-------------|
| `AcademicYears_AY` | AY | `AY_Id` | `AY_TenantId` | Status-based |
| `Programs_P` | P | `P_Id` | `P_TenantId` | `P_Status` |
| `Courses_C` | C | `C_Id` | `C_TenantId` | `C_DeletedAt` + `C_Status` |
| `Subjects_SB` | SB | `SB_Id` | `SB_TenantId` | Hard delete only |
| `CourseSubjects_CS` | CS | Composite: `CS_CourseId + CS_SubjectId` | Via Course | Hard delete only |
| `Batches_BT` | BT | `BT_Id` | `BT_TenantId` | `BT_Status` |
| `BatchStudents_BS` | BS | Composite: `BS_BatchId + BS_StudentId` | Via Batch | `BS_LeftAt` |
| `Classrooms_CR` | CR | `CR_Id` | `CR_TenantId` | Status-based |
| `Timetables_TT` | TT | `TT_Id` | `TT_TenantId` | Hard delete |
| `AttendanceSessions_AS` | AS | `AS_Id` | `AS_TenantId` | Hard delete |
| `AttendanceRecords_AR` | AR | `AR_Id` | Via Session | Hard delete |
| `Enrollments_E` | E | `E_Id` | `E_TenantId` | `E_Status` |

#### Category C: Students & Guardians

| Table | Suffix | PK | TenantId | Soft Delete |
|-------|--------|----|----------|-------------|
| `Students_S` | S | `S_Id` | `S_TenantId` | `S_DeletedAt` + `S_Status` |
| `Guardians_G` | G | `G_Id` | `G_TenantId` | `G_DeletedAt` (model) |
| `Students_Guardians` | SG | `SG_Id` | Via Student | Hard delete |
| `StudentGuardians_SG` | SG | Composite: `SG_StudentId + SG_GuardianId` | Via Student | Hard delete |
| `AdmissionApplications_AA` | AA | `AA_Id` | `AA_TenantId` | `AA_Status` |
| `AdmissionApplicationDocuments_AAD` | AAD | `AAD_Id` | Via Application | Hard delete |
| `AdmissionNumberCounters_ANC` | ANC | Composite: `ANC_TenantId + ANC_Year` | `ANC_TenantId` | Counter |

#### Category D: Staff

| Table | Suffix | PK | TenantId | Soft Delete |
|-------|--------|----|----------|-------------|
| `Staff_ST` | ST | `ST_Id` | `ST_TenantId` | `ST_DeletedAt` + `ST_Status` |
| `Departments_D` | D | `D_Id` | `D_TenantId` | Hard delete |
| `Designations_DS` | DS | `DS_Id` | `DS_TenantId` | Hard delete |

#### Category E: Examinations

| Table | Suffix | PK | TenantId | Soft Delete |
|-------|--------|----|----------|-------------|
| `ExamTypes_ET` | ET | `ET_Id` | `ET_TenantId` | Hard delete |
| `Exams_EX` | EX | `EX_Id` | `EX_TenantId` | `EX_Status` |
| `ExamSubjects_ES` | ES | `ES_Id` | Via Exam | Hard delete |
| `ExamSchedules_ESC` | ESC | `ESC_Id` | Via ExamSubject | Hard delete |
| `Marks_M` | M | `M_Id` | Via ExamSubject | Hard delete |
| `Results_R` | R | `R_Id` | Via Exam | Hard delete |
| `GradeScales_GS` | GS | `GS_Id` | `GS_TenantId` | Hard delete |
| `GradeScaleItems_GSI` | GSI | `GSI_Id` | Via GradeScale | Hard delete |

#### Category F: Fees & Finance

| Table | Suffix | PK | TenantId | Soft Delete |
|-------|--------|----|----------|-------------|
| `FeeCategories_FC` | FC | `FC_Id` | `FC_TenantId` | Hard delete |
| `FeeStructures_FS` | FS | `FS_Id` | `FS_TenantId` | `FS_IsActive` |
| `FeeStructureItems_FSI` | FSI | `FSI_Id` | Via FeeStructure | Hard delete |
| `StudentFeeAssignments_SFA` | SFA | `SFA_Id` | Via Student/FeeStructure | `SFA_Status` |
| `FeeInvoices_FI` | FI | `FI_Id` | `FI_TenantId` | `FI_Status` |
| `FeeInvoiceItems_FII` | FII | `FII_Id` | Via FeeInvoice | Hard delete |
| `Discounts_DIS` | DIS | `DIS_Id` | `DIS_TenantId` | `DIS_IsActive` |
| `PaymentMethods_PM` | PM | `PM_Id` | `PM_TenantId` | `PM_IsActive` |
| `Payments_PAY` | PAY | `PAY_Id` | `PAY_TenantId` | `PAY_Status` |
| `PaymentAllocations_PA` | PA | `PA_Id` | Via Payment | Hard delete |
| `Refunds_RF` | RF | `RF_Id` | `RF_TenantId` | `RF_Status` |

#### Category G: Expenses

| Table | Suffix | PK | TenantId | Soft Delete |
|-------|--------|----|----------|-------------|
| `ExpenseCategories_EC` | EC | `EC_Id` | `EC_TenantId` | Hard delete |
| `Vendors_V` | V | `V_Id` | `V_TenantId` | Hard delete |
| `Expenses_EXP` | EXP | `EXP_Id` | `EXP_TenantId` | Hard delete |

#### Category H: Documents & Custom Fields

| Table | Suffix | PK | TenantId | Soft Delete |
|-------|--------|----|----------|-------------|
| `Documents_DOC` | DOC | `DOC_Id` | `DOC_TenantId` | Immutable |
| `DocumentTypes_DT` | DT | `DT_Id` | `DT_TenantId` | Hard delete |
| `EntityDocuments_ED` | ED | `ED_Id` | `ED_TenantId` | Hard delete |
| `CustomFields_CF` | CF | `CF_Id` | `CF_TenantId` | `CF_IsActive` |
| `CustomFieldValues_CFV` | CFV | `CFV_Id` | Via CustomField | Hard delete |

#### Category I: Notifications & Audit

| Table | Suffix | PK | TenantId | Soft Delete |
|-------|--------|----|----------|-------------|
| `Announcements_ANN` | ANN | `ANN_Id` | `ANN_TenantId` | Expires via `ANN_ExpiresAt` |
| `Notifications_N` | N | `N_Id` | `N_TenantId` | Immutable log |
| `NotificationTemplates_NT` | NT | `NT_Id` | `NT_TenantId` | `NT_IsActive` |
| `ActivityLogs_ACL` | ACL | `ACL_Id` | `ACL_TenantId` | Append-only |
| `AuditLogs_AL` | AL | `AL_Id` | `AL_TenantId` | Append-only |

---

## 4. Entity Relationship Diagram

### 4.1 Core Academic Flow

```
                    ┌──────────────────┐
                    │   Organization   │
                    │   (Tenant Root)  │
                    └────────┬─────────┘
                             │ tenant scope
            ┌────────────────┼────────────────┐
            │                │                │
     ┌──────▼──────┐  ┌─────▼─────┐  ┌──────▼──────┐
     │   Branch    │  │  Program  │  │AcademicYear │
     │ (Branches_B)│  │(Programs_P│  │(AcdYears_AY)│
     └──┬──┬──┬────┘  └─────┬─────┘  └──┬─────┬────┘
        │  │  │             │            │     │
        │  │  │        ┌────▼────┐       │     │
        │  │  │        │ Course  │◄──────┘     │
        │  │  │        │(Courses │             │
        │  │  │        │   _C)   │             │
        │  │  │        └──┬───┬──┘             │
        │  │  │           │   │                │
        │  │  │     ┌─────┘   └──────┐         │
        │  │  │     │                │         │
        │  │  │     ▼                ▼         │
        │  │  │ ┌──────────┐  ┌───────────┐   │
        │  │  │ │CourseSubj│  │   Batch   │◄──┘
        │  │  │ │(CS_Course│  │(Batches_BT│
        │  │  │ │CS_Subj)  │  └─────┬─────┘
        │  │  │ └────┬─────┘        │
        │  │  │      │        ┌─────┼────────┐
        │  │  │      ▼        │     │        │
        │  │  │ ┌─────────┐   │     │        │
        │  │  │ │ Subject │   │     │        │
        │  │  │ │(Subj_SB)│   │     │        │
        │  │  │ └─────────┘   │     │        │
        │  │  │               │     │        │
        │  │  │    ┌──────────┘     │        │
        │  │  │    │                │        │
        │  │  │    ▼                ▼        │
        │  │  │┌──────────┐  ┌───────────┐  │
        │  │  ││Timetable │  │ Enrollment│  │
        │  │  ││(TT_Branch│  │(E_Student │  │
        │  │  ││ TT_Batch │  │ E_Course  │  │
        │  │  ││ TT_Subj  │  │ E_Batch   │  │
        │  │  ││ TT_Staff │  │ E_AcdYear)│  │
        │  │  ││ TT_Class)│  └─────┬─────┘  │
        │  │  │└──────────┘        │        │
        │  │  │                    │        │
        │  │  │           ┌────────▼──────┐ │
        │  │  │           │   Student     │ │
        │  │  │           │  (Students_S) │ │
        │  │  │           └──┬───┬───┬───┘ │
        │  │  │              │   │   │     │
        │  │  ▼    ┌─────────┘   │   └─────┼──────┐
        │  │┌──────▼──────┐      │         │      │
        │  ││  Classroom  │      │         │      │
        │  ││(Classrooms_ │      │         │      │
        │  ││     CR)     │      │         │      │
        │  │└─────────────┘      │         │      │
        │  │                     │         │      │
        │  │     ┌───────────────┘         │      │
        │  │     │                         │      │
        │  ▼     ▼                         ▼      ▼
        │┌──────────┐            ┌──────────┐ ┌──────────┐
        ││Department│            │ Guardian │ │ Fee      │
        ││(Dept_D)  │            │(Guard_G) │ │ Invoice  │
        │└──────────┘            └──────────┘ │(Fees_FI) │
        │                                     └──────────┘
        ▼
  ┌──────────┐
  │  Staff   │
  │(Staff_ST)│
  └──────────┘
```

### 4.2 Finance Chain

```
FeeCategory (FC)
      │
      ├── FeeStructureItem (FSI) ── FeeStructure (FS) ── AcademicYear, Course, Batch
      │
      └── FeeInvoiceItem (FII) ── FeeInvoice (FI) ── Student
                                      │
                                      └── PaymentAllocation (PA) ── Payment (PAY) ── Student, PaymentMethod
                                                            │
                                                            └── Refund (RF) ── Payment, Student

Discount (DIS) ── used in FeeInvoiceItem (FII_DiscountAmount) [no FK, just amount]
```

### 4.3 Examination Chain

```
ExamType (ET)
      │
      └── Exam (EX) ── AcademicYear, Course, Batch
            │
            ├── ExamSubject (ES) ── Subject
            │       │
            │       ├── ExamSchedule (ESC) ── Classroom
            │       │
            │       └── Mark (M) ── Student, GradeScaleItem
            │
            └── Result (R) ── Student

GradeScale (GS) ── GradeScaleItem (GSI) ── Mark (M_GradeScaleItemId)
```

### 4.4 Attendance Chain

```
AttendanceSession (AS) ── Branch, Batch, Subject, Staff
      │
      └── AttendanceRecord (AR) ── Student
```

---

## 5. Module Breakdown

### 5.1 Module: Dashboard & Authentication

| Component | File |
|-----------|------|
| Controller | `IMS.Web/Controllers/HomeController.cs` |
| Services | `IUserSessionService`, `IRedisService` |
| Views | `Home/Index`, `Home/Dashboard`, `Home/AccessDenied` |
| Auth | Keycloak OpenID Connect via `KeycloakExtensions.cs` |
| Permissions | Custom `[Permission("...")]` attribute via `PermissionAuthorizationHandler` |

**How it works:**
1. User logs in via Keycloak → JWT token issued
2. `HomeController.Login` validates token, fetches user profile from external API
3. User profile + permissions cached in Redis (key: `IMS:users:{username}`, 30min TTL)
4. Every controller reads `tenant_id` and `user_id` from JWT claims
5. Permission checks via `[Permission("Master.Branch.Create")]` → `PermissionAuthorizationHandler` reads from Redis

### 5.2 Module: Students

| Component | File |
|-----------|------|
| Controller | `IMS.Web/Controllers/StudentsController.cs` |
| Service | `IMS.Services/StudentService.cs` |
| DAL | `IMS.DAL/StudentDAL.cs` |
| Views | `Students/Index`, `Create`, `Edit`, `Details`, `_Form` |
| JS | `wwwroot/js/students.js` |
| SPs | `SP_Students_Create`, `SP_Students_Update`, `SP_Students_SoftDelete`, etc. |

**Entity:** `Students_S`

| FK | References | Required | UI |
|----|-----------|----------|-----|
| `S_BranchId` | `Branches_B.B_Id` | Yes | Branch dropdown (HardcodedMasterData) |
| `S_ClassId` | (Class GUID) | No | Class dropdown (HardcodedMasterData) |
| `S_SectionId` | (Section GUID) | No | Section dropdown (HardcodedMasterData) |
| `S_UserId` | (Auth user) | No | Not exposed in UI |

**Guardian relationship:** Many-to-Many via `Students_Guardians` join table.

The Student form renders a **Guardians section** with:
- Dynamic add/remove rows (jQuery)
- Autocomplete search (`/Students/SearchGuardians`) to link existing guardians
- OR inline creation of new guardians
- All saved in a **single transaction** (`CreateStudentWithGuardiansAsync`)

### 5.3 Module: Admissions

| Component | File |
|-----------|------|
| Controller | `IMS.Web/Controllers/AdmissionApplicationController.cs` |
| Service | `IMS.Services/AdmissionApplicationService.cs` |
| DAL | `IMS.DAL/AdmissionApplicationDAL.cs` |
| Views | `AdmissionApplication/Index`, `Create`, `Edit`, `Details`, `_Form` |
| SP | `USP_AdmissionApplications_AA` |

**Entity:** `AdmissionApplications_AA`

| FK | References | Required |
|----|-----------|----------|
| `AA_BranchId` | `Branches_B.B_Id` | Yes |
| `AA_CourseId` | `Courses_C.C_Id` | No |
| `AA_AcademicYearId` | `AcademicYears_AY.AY_Id` | Yes |

**Status workflow:** `Submitted` → `UnderReview` → `Approved` / `Rejected` / `Waitlisted`

**Auto-generated:** `AA_ApplicationNumber` in format `APP-YY-NNNN`

### 5.4 Module: Batches

| Component | File |
|-----------|------|
| Controller | `IMS.Web/Controllers/BatchController.cs` |
| Service | `IMS.Services/BatchService.cs` |
| DAL | `IMS.DAL/BatchDAL.cs` |
| Views | `Batch/Index`, `Create`, `Edit`, `Details`, `_Form` |
| JS | `wwwroot/js/batches.js` |
| SP | `USP_Batches_BT` |

**Entity:** `Batches_BT`

| FK | References | Required |
|----|-----------|----------|
| `BT_BranchId` | `Branches_B.B_Id` | Yes |
| `BT_CourseId` | `Courses_C.C_Id` | Yes |
| `BT_AcademicYearId` | `AcademicYears_AY.AY_Id` | Yes |

**Related:** `BatchStudents_BS` tracks student-batch membership. `Enrollments_E` is the formal enrollment record.

### 5.5 Module: Course Subjects

| Component | File |
|-----------|------|
| Controller | `IMS.Web/Controllers/CourseSubjectController.cs` |
| Service | `IMS.Services/CourseSubjectService.cs` |
| DAL | `IMS.DAL/CourseSubjectDAL.cs` |
| Views | `CourseSubject/Index`, `Create`, `Edit`, `_Form` |
| SP | `USP_CourseSubjects_CS` |

**Entity:** `CourseSubjects_CS` (junction table — composite PK)

| FK | References | Required |
|----|-----------|----------|
| `CS_CourseId` | `Courses_C.C_Id` | Yes (PK) |
| `CS_SubjectId` | `Subjects_SB.SB_Id` | Yes (PK) |

**Additional attributes:** `CS_SequenceNo`, `CS_IsMandatory`, `CS_MaxMarks`, `CS_PassMarks`

### 5.6 Module: Enrollments

| Component | File |
|-----------|------|
| Controller | `IMS.Web/Controllers/EnrollmentController.cs` |
| Service | `IMS.Services/EnrollmentService.cs` |
| DAL | `IMS.DAL/EnrollmentDAL.cs` |
| Views | `Enrollment/Index`, `Create`, `Edit`, `Details`, `_Form` |
| SP | `USP_Enrollments_E` |

**Entity:** `Enrollments_E`

| FK | References | Required |
|----|-----------|----------|
| `E_StudentId` | `Students_S.S_Id` | Yes |
| `E_AcademicYearId` | `AcademicYears_AY.AY_Id` | Yes |
| `E_CourseId` | `Courses_C.C_Id` | Yes |
| `E_BatchId` | `Batches_BT.BT_Id` | Yes |

**Business rule:** Same student cannot be enrolled twice in the same batch (`IsDuplicateAsync` check).

**Auto-generated:** `E_EnrollmentNumber` in format `ENR-YY-NNNN`

### 5.7 Module: Timetable

| Component | File |
|-----------|------|
| Controller | `IMS.Web/Controllers/TimetableController.cs` |
| Service | `IMS.Services/TimetableService.cs` |
| DAL | `IMS.DAL/TimetableDAL.cs` |
| Views | `Timetable/Index`, `Create`, `Edit`, `Details`, `_Form` |
| SP | `USP_Timetables_TT` |

**Entity:** `Timetables_TT`

| FK | References | Required |
|----|-----------|----------|
| `TT_BranchId` | `Branches_B.B_Id` | Yes |
| `TT_BatchId` | `Batches_BT.BT_Id` | Yes |
| `TT_SubjectId` | `Subjects_SB.SB_Id` | Yes |
| `TT_StaffId` | `Staff_ST.ST_Id` | Yes |
| `TT_ClassroomId` | `Classrooms_CR.CR_Id` | No |

**Business rule:** Time slot conflict detection — cannot overlap with existing entries for same batch + day of week. Checked via `CheckConflictAsync` endpoint.

### 5.8 Module: Master Management (Generic CRUD)

| Component | File |
|-----------|------|
| Controller | `IMS.Web/Controllers/MasterController.cs` (route: `/Master/{entityType}`) |
| Service | `IMS.Services/Common/MasterService.cs` |
| DAL | `IMS.DAL/Common/MasterDAL.cs` |
| Views | `Master/Menu` (overview), `Master/Index` (dynamic per entity) |
| Config | `IMS.Models/Common/Master/MasterConfigRegistry.cs` |

**20 entity types** served by a single controller. Each entity has a single multi-action stored procedure (`USP_{EntityName}`) that handles GetAll, GetById, Insert, Update, Delete, ExistsByField.

**Master Entity Relationships (dropdown FK lookups):**

```
Program (P_Id)
    │
    └── Course.C_ProgramId → Course belongs to a Program

Branch (B_Id)
    │
    ├── Department.D_BranchId → Department belongs to a Branch
    │
    └── Classroom.CR_BranchId → Classroom belongs to a Branch
```

### 5.9 Module: Generic Dropdowns

| Component | File |
|-----------|------|
| Controller | `IMS.Web/Controllers/DropdownController.cs` |
| Service | `IMS.Services/Common/DropdownService.cs` |
| DAL | `IMS.DAL/Common/DropdownDAL.js` |
| SP | `USP_GenericDropdown` (parameterized) |
| Config | `IMS.Models/Common/Dropdown/DropdownConfigRegistry.cs` |
| JS | `wwwroot/js/common/dropdown.js` |

**5 registered dropdown entity types** (INT-keyed, for Inventory/Finance):

| EntityType | Table | Display |
|-----------|-------|---------|
| PaymentMode | `PaymentMode_PM` | `PM_ModeName` |
| ProductCategory | `ProductCategory_PC` | `PC_CategoryName` |
| ProductBrand | `ProductBrand_PB` | `PB_BrandName` |
| ProductUnit | `ProductUnit_PU` | `PU_UnitName` |
| VendorCategory | `VendorCategories_VC` | `VC_CategoryName` |

**JS API:** `Dropdown.load({ element: "#ddl", entityType: "PaymentMode" })` → AJAX → `GET /Dropdown/GetDropdown`

---

## 6. Stored Procedures Map

### 6.1 Master CRUD SPs (Generic Multi-Action)

Each SP handles all CRUD operations via an `@Action` parameter:

| SP Name | Entity |
|---------|--------|
| `USP_Branches_B` | Branch |
| `USP_Courses_C` | Course |
| `USP_AcademicYears_AY` | AcademicYear |
| `USP_Departments_D` | Department |
| `USP_Designations_DS` | Designation |
| `USP_DocumentTypes_DT` | DocumentType |
| `USP_ExamTypes_ET` | ExamType |
| `USP_ExpenseCategories_EC` | ExpenseCategory |
| `USP_FeeCategories_FC` | FeeCategory |
| `USP_GradeScales_GS` | GradeScale |
| `USP_Classrooms_CR` | Classroom |
| `USP_PaymentMethods_PM` | PaymentMethod |
| `USP_Discounts_DIS` | Discount |
| `USP_Subjects_SB` | Subject |
| `USP_Programs_P` | Program |
| `USP_Vendors_V` | Vendor |
| `USP_Staff_ST` | Staff |
| `USP_Students_S` | Student (lookup) |
| `USP_Batches_BT` | Batch (lookup) |
| `USP_NotificationTemplates_NT` | NotificationTemplate |
| `USP_Bank` | BankMaster |

### 6.2 Academic Module SPs

| SP Name | Entity | Actions |
|---------|--------|---------|
| `USP_Batches_BT` | Batch | GetAll, GetById, GetPaged, Insert, Update, Delete, ExistsByCode |
| `USP_CourseSubjects_CS` | CourseSubject | GetAll, GetByCourseId, GetById, Insert, Update, Delete, Exists |
| `USP_Enrollments_E` | Enrollment | GetById, GetPaged, IsDuplicate, Insert, Update, Delete |
| `USP_Timetables_TT` | Timetable | GetAll, GetById, CheckConflict, Insert, Update, Delete |
| `USP_AdmissionApplications_AA` | AdmissionApp | GetById, GetPaged, ExistsByNumber, Insert, Update, Delete, Review |

### 6.3 Student Module SPs

| SP Name | Action |
|---------|--------|
| `SP_Students_GetById` | Get single student |
| `SP_Students_GetPaged` | Paged listing with filters |
| `SP_Students_Create` | Insert (auto-generates AdmissionNumber) |
| `SP_Students_Update` | Update record |
| `SP_Students_SoftDelete` | Set S_DeletedAt |
| `SP_Students_CheckAdmissionNumberExists` | Uniqueness check |
| `SP_Students_CheckStudentCodeExists` | Uniqueness check |
| `SP_Guardians_Create` | Insert guardian |
| `SP_Guardians_Search` | Search guardians (autocomplete) |
| `SP_Guardians_GetById` | Get single guardian |
| `SP_StudentGuardians_Add` | Link guardian to student |
| `SP_StudentGuardians_GetByStudentId` | Get all guardians for student |
| `SP_StudentGuardians_RemoveByStudent` | Remove all guardian links |

### 6.4 Utility SPs

| SP Name | Purpose |
|---------|---------|
| `USP_GenericDropdown` | Dynamic dropdown for any table (parameterized) |

---

## 7. Data Flow: How Modules Connect

### 7.1 Student Lifecycle (End-to-End)

```
1. ADMISSION APPLICATION
   AdmissionApplicationController.Create()
     → Applicant fills form (Branch, Course, AcademicYear, personal info)
     → Status: "Submitted"
     → Review action: Approve / Reject / Waitlist
     → Approved applications proceed to step 2

2. STUDENT CREATION
   StudentsController.Create()
     → Admin fills student form (Branch, Class, Section, personal info)
     → Guardians linked (existing or new, in single transaction)
     → Auto-generated: AdmissionNumber (ADM-YYYY-NNNN), StudentCode (STU-YYMMDD-XXXX)
     → Student record created in Students_S

3. ENROLLMENT
   EnrollmentController.Create()
     → Admin selects Student, AcademicYear, Course, Batch
     → Enrollments_E record created
     → E_EnrollmentNumber auto-generated (ENR-YY-NNNN)
     → Student can now appear in Timetables, Attendance, Fees

4. BATCH MEMBERSHIP
   BatchStudents_BS (implicit via Enrollment)
     → Student belongs to a Batch
     → Batch belongs to a Course + AcademicYear + Branch
     → Timetable entries are per-Batch

5. TIMETABLE
   TimetableController.Create()
     → Admin selects Batch, Subject, Staff, Classroom, Day, Time
     → Conflict detection prevents overlapping slots
     → Student sees timetable based on their Batch enrollment

6. ATTENDANCE (planned)
   AttendanceSession → AttendanceRecord
     → Session is per-Batch+Subject+Date
     → Record is per-Student within that Session

7. EXAMINATIONS (planned)
   Exam → ExamSubject → Mark → Result
     → Exam is per-Batch+Course+AcademicYear
     → Marks per-Student per-Subject
     → Results computed from Marks + GradeScale

8. FEES (planned)
   FeeStructure → FeeInvoice → Payment
     → FeeStructure defines fees per Course/Batch/AcademicYear
     → FeeInvoice generated per-Student
     → Payment allocated to Invoice(s)
```

### 7.2 Master Data Dependencies

Before any academic module can function, these master entities must be configured:

```
REQUIRED (minimum setup):
├── Branch       → Every entity needs a Branch
├── Program      → Course requires a Program
├── Course       → Batch, Enrollment, Timetable, Fees, Exams need a Course
├── AcademicYear → Batch, Enrollment, Fees, Exams need an Academic Year
├── Subject      → CourseSubject, Timetable, Attendance, Exams need Subjects
├── Staff        → Timetable, Attendance need Staff/Teachers
└── Classroom    → Timetable, Exams need Classrooms

RECOMMENDED:
├── Department   → Staff belongs to a Department
├── Designation  → Staff has a Designation
├── FeeCategory  → Fee structure items need categories
├── PaymentMethod → Payments need a method
├── GradeScale   → Exams need grading
└── ExamType     → Exams need a type (Midterm, Final, etc.)
```

### 7.3 Dropdown Population Flow

Three systems populate dropdowns in the UI:

**System A: Master Service (GUID-keyed — for academic entities)**
```
Service.PopulateDropdowns(vm)
  → GetMasterSelectList("Course", selectedId)
    → _masterService.GetAll("Course")
      → MasterDAL.GetAll(MasterConfig)
        → USP_Courses_C (GetAll action)
  → Returns List<SelectListItem>
  → Rendered via @Html.DropDownListFor()
```

**System B: Dropdown Service (INT-keyed — for inventory/finance)**
```
JavaScript: Dropdown.load({ element: "#ddl", entityType: "PaymentMode" })
  → AJAX GET /Dropdown/GetDropdown?entityType=PaymentMode
    → DropdownService.GetDropdown(request)
      → DropdownDAL.GetDropdown(config, request)
        → USP_GenericDropdown (parameterized)
  → Returns JSON { Value, Text, Code }
  → Rendered client-side
```

**System C: Hardcoded Data (static — temporary for Student module)**
```
HardcodedMasterData.GetBranchSelectList()
  → Returns hardcoded GUID lists
  → Used for: Branch, Class, Section, Gender, BloodGroup
  → TODO: Replace with database-backed data
```

---

## 8. Key Patterns

### 8.1 Multi-Tenancy

```
Every table: *_TenantId column (UNIQUEIDENTIFIER)
Every SP: WHERE *_TenantId = @TenantId (first filter)
Every controller: CurrentTenantId from JWT "tenant_id" claim
Every service call: tenantId passed as first parameter
```

### 8.2 Soft Delete (Three Patterns)

| Pattern | Column | Used By |
|---------|--------|---------|
| **Timestamp** | `*_DeletedAt` (datetime, nullable) | Organizations, Branches, Courses, Students, Staff |
| **Boolean** | `*_IsActive` (bit) | Master entities (Bank, Discount, FeeStructure, etc.) |
| **Status** | `*_Status` (nvarchar) | Batches, Enrollments, Exams, Payments, Admissions |

### 8.3 Auto-Generated Numbers

| Entity | Format | Example |
|--------|--------|---------|
| Admission Number | `ADM-YYYY-NNNN` | ADM-2026-0001 |
| Student Code | `STU-YYMMDD-XXXX` | STU-260830-A1B2 |
| Enrollment Number | `ENR-YY-NNNN` | ENR-26-0001 |
| Application Number | `APP-YY-NNNN` | APP-26-0001 |

### 8.4 Generic CRUD Pattern

The Master module uses a single controller + service + DAL for all 20 entity types:

```
MasterController (1 controller)
  → MasterService (1 service)
    → MasterDAL (1 DAL)
      → MasterConfigRegistry (20 entity configs)
        → USP_{EntityName} (1 SP per entity, multi-action)
```

Each config defines: table name, key column, fields, validation rules, dropdown lookups, permissions.

### 8.5 AJAX Form Pattern

All form submissions follow this pattern:
```javascript
$("form").on("submit", function(e) {
    e.preventDefault();
    $.ajax({
        url: $(this).attr("action"),
        type: "POST",
        data: $(this).serialize(),  // includes __RequestVerificationToken
        success: function(r) {
            if (r.success) {
                toastr.success(r.message);
                setTimeout(function() { window.location.href = "/Controller/Index"; }, 700);
            } else {
                toastr.error(r.message);
            }
        }
    });
});
```

Server returns: `Json(new { success = true/false, message = "...", id = guid })`

### 8.6 Validation Pattern

- **Client-side:** JavaScript validation in `batches.js`, `students.js`, or inline `<script>` blocks
- **Server-side:** Service layer checks (uniqueness, required fields, business rules)
- **No DataAnnotations:** Validation is NOT done via model attributes
- **Error display:** `.field-error` divs with `data-for` attribute, populated by JS

---

## 9. Implementation Status

### 9.1 Fully Implemented Modules

| Module | Controller | Service | DAL | Views | JS | SPs |
|--------|-----------|---------|-----|-------|----|----|
| Dashboard | ✅ | ✅ | — | ✅ | — | — |
| Students | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| Admissions | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| Batches | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| CourseSubjects | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| Enrollments | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| Timetable | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| Master Mgmt | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| Dropdowns | ✅ | ✅ | ✅ | — | ✅ | ✅ |

### 9.2 Database-Only (No Application Code)

| Module | Tables Exist | SPs Exist | Controller | Service | Views |
|--------|-------------|-----------|------------|---------|-------|
| Attendance | ✅ | ❌ | ❌ | ❌ | ❌ |
| Examinations | ✅ | ❌ | ❌ | ❌ | ❌ |
| Results | ✅ | ❌ | ❌ | ❌ | ❌ |
| Fees & Payments | ✅ | ❌ | ❌ | ❌ | ❌ |
| Expenses | ✅ | ❌ | ❌ | ❌ | ❌ |
| Documents | ✅ | ❌ | ❌ | ❌ | ❌ |
| Notifications | ✅ | ❌ | ❌ | ❌ | ❌ |

### 9.3 Planned Areas (Sidebar Wired, No Code)

| Area | Route | Status |
|------|-------|--------|
| Teachers | `Area/Teacher/Teacher` | Planned |
| Courses (standalone) | `Area/Course/Course` | Planned |
| Subjects (standalone) | `Area/Subject/Subject` | Planned |
| Attendance | `Area/Attendance/Attendance` | Planned |
| Fees & Payments | `Area/Fees/Fees` | Planned |
| Examinations | `Area/Exam/Exam` | Planned |
| Results | `Area/Result/Result` | Planned |

### 9.4 Known Issues

1. **Staff_ST table** has no PRIMARY KEY constraint defined in the SQL script
2. **Discounts_DIS** is orphaned — no FK from other tables references it
3. **Two student-guardian link tables** exist (`StudentGuardians_SG` and `Students_Guardians`) — code uses the newer `Students_Guardians`
4. **HardcodedMasterData** for Branch/Class/Section in Student module — should be replaced with database-backed data
5. **Dropdown framework** uses INT keys — cannot serve GUID-keyed academic entities directly

---

## Appendix A: File Reference

### Controllers
| File | Path |
|------|------|
| HomeController | `IMS.Web/Controllers/HomeController.cs` |
| StudentsController | `IMS.Web/Controllers/StudentsController.cs` |
| AdmissionApplicationController | `IMS.Web/Controllers/AdmissionApplicationController.cs` |
| BatchController | `IMS.Web/Controllers/BatchController.cs` |
| CourseSubjectController | `IMS.Web/Controllers/CourseSubjectController.cs` |
| EnrollmentController | `IMS.Web/Controllers/EnrollmentController.cs` |
| TimetableController | `IMS.Web/Controllers/TimetableController.cs` |
| MasterController | `IMS.Web/Controllers/MasterController.cs` |
| DropdownController | `IMS.Web/Controllers/DropdownController.cs` |

### Services
| File | Path |
|------|------|
| StudentService | `IMS.Services/StudentService.cs` |
| AdmissionApplicationService | `IMS.Services/AdmissionApplicationService.cs` |
| BatchService | `IMS.Services/BatchService.cs` |
| CourseSubjectService | `IMS.Services/CourseSubjectService.cs` |
| EnrollmentService | `IMS.Services/EnrollmentService.cs` |
| TimetableService | `IMS.Services/TimetableService.cs` |
| MasterService | `IMS.Services/Common/MasterService.cs` |
| DropdownService | `IMS.Services/Common/DropdownService.cs` |

### DALs
| File | Path |
|------|------|
| StudentDAL | `IMS.DAL/StudentDAL.cs` |
| AdmissionApplicationDAL | `IMS.DAL/AdmissionApplicationDAL.cs` |
| BatchDAL | `IMS.DAL/BatchDAL.cs` |
| CourseSubjectDAL | `IMS.DAL/CourseSubjectDAL.cs` |
| EnrollmentDAL | `IMS.DAL/EnrollmentDAL.cs` |
| TimetableDAL | `IMS.DAL/TimetableDAL.cs` |
| MasterDAL | `IMS.DAL/Common/MasterDAL.cs` |
| DropdownDAL | `IMS.DAL/Common/DropdownDAL.cs` |
| DBHelper | `IMS.DAL/Common/DBHelper.cs` |

### Config Registries
| File | Path |
|------|------|
| MasterConfigRegistry | `IMS.Models/Common/Master/MasterConfigRegistry.cs` |
| DropdownConfigRegistry | `IMS.Models/Common/Dropdown/DropdownConfigRegistry.cs` |

### DI Registration
| File | Path |
|------|------|
| ServiceLayerExtensions | `IMS.Web/Extensions/ServiceLayerExtensions.cs` |
| DALServiceExtensions | `IMS.Web/Extensions/DALServiceExtensions.cs` |
| KeycloakExtensions | `IMS.Web/Extensions/KeycloakExtensions.cs` |

### Database Scripts
| File | Path |
|------|------|
| Main Schema + Master SPs | `database/script_29_08_2026.sql` |
| Academic SPs | `database/academic_sp.sql` |
| This Document | `database/SYSTEM_ARCHITECTURE.md` |
