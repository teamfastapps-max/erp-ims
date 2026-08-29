# IMS Database Architecture Review
**Scope:** Tables, keys, constraints, normalization, relationships only. No backend/API/app-layer discussion.
**Approach:** Preserve existing structure. Small improvements + targeted additions over redesign.

---

## Executive Summary

The existing schema is **already close to a generic institution model**. The core academic chain —

```
Organizations_O (tenant) → Branches_B (campus) → Programs_P (optional) → Courses_C → Batches_BT → Students_S
```

— maps cleanly onto every institution type you listed:

| Institution type | Program_P | Courses_C | Batches_BT |
|---|---|---|---|
| Nursery/Primary/Secondary school | *(unused, null)* | "Grade 5" | "Grade 5 – Section A" |
| Higher secondary / college | "B.Sc." | "B.Sc. Computer Science" | "B.Sc. CS 2024 Batch A" |
| Coaching centre | *(unused, null)* | "NEET 2026 Course" | "NEET Batch – Morning" |

`C_ProgramId` is nullable, so coaching centres and schools simply skip Programs. This is a genuinely reusable model — **do not add separate Grade/Section tables**; that would duplicate what Course/Batch already do.

Two things need attention before anything else:
1. **`Staff_ST` has no primary key, no foreign keys, and no unique constraint** — this is a data-integrity gap, not a design opinion. Fix first.
2. **`Discounts_DIS` is defined but never referenced by any FK** — discount amounts are stored as raw numbers on `StudentFeeAssignments_SFA` / `FeeInvoiceItems_FII` with no traceability to which discount was applied.

Everything else below is either a confirmation that the design is sound (KEEP) or a small, additive improvement.

---

## A. Current Table Assessment

### Tenancy / Org structure
| Table | Status | Purpose | Recommendation |
|---|---|---|---|
| Organizations_O | KEEP | Tenant root | None. Timezone/currency/status well-modeled. |
| OrganizationSettings_OS | KEEP | Tenant-level key/value config | Already the correct mechanism for "institution type" and other per-tenant flags — see §3. |
| Branches_B | KEEP | Campus | Correctly separates tenant from campus; soft-delete present. |
| CustomFields_CF / CustomFieldValues_CFV | KEEP | Per-tenant EAV extension | Exactly the right pattern for institution-specific fields without schema changes. |
| Documents_DOC / DocumentTypes_DT / EntityDocuments_ED | KEEP | Generic polymorphic document storage | Already entity-type generic (student/staff/guardian/application/other). |

### Academic structure
| Table | Status | Purpose | Recommendation |
|---|---|---|---|
| AcademicYears_AY | KEEP | Tenant academic year | Good — `AY_IsCurrent` flag and date-range check constraint are correct patterns. |
| Programs_P | KEEP | Optional grouping above Course | Nullable link from Course is the right generic design. |
| Courses_C | KEEP | Grade / Programme / Coaching course | Generic label works across institution types. |
| CourseSubjects_CS | IMPROVE | Course curriculum (subject list, marks, sequence) | Not versioned by academic year — see §C. |
| Subjects_SB | KEEP | Tenant subject catalog | Fine as shared master data. |
| Batches_BT | KEEP | Section / Cohort / Coaching batch | Correctly tenant+branch+course+year scoped. |
| BatchStudents_BS | IMPROVE | Batch membership history | Overlaps with `Enrollments_E`; see §C for how to divide responsibility. |
| Classrooms_CR | KEEP | Physical room master | Used consistently by Timetables and ExamSchedules. |
| Timetables_TT | KEEP | Recurring weekly schedule | Well designed — `EffectiveFrom/To` correctly preserves history across timetable revisions. |

### Students & guardians
| Table | Status | Purpose | Recommendation |
|---|---|---|---|
| Students_S | KEEP | Core student record | Good soft-delete + unique codes per tenant. |
| Enrollments_E | IMPROVE | Formal course/year enrollment record | See §C — clarify vs BatchStudents_BS. |
| Guardians_G | KEEP | Tenant-level guardian record | Correctly not branch-scoped — one guardian can span branches/children. |
| StudentGuardians_SG | KEEP | Student↔Guardian junction | Proper M:N with relationship/primary/emergency flags. |
| AdmissionApplications_AA | KEEP | Pre-enrollment pipeline | Good separation from Students_S — application ≠ enrolled student. |
| AdmissionApplicationDocuments_AAD | KEEP (see §C) | Application document verification | Slight overlap with EntityDocuments_ED — intentional, documented below. |

### Staff
| Table | Status | Purpose | Recommendation |
|---|---|---|---|
| Staff_ST | **IMPROVE — Critical** | Employee/teacher record | **Missing PRIMARY KEY, all FOREIGN KEYS, and a unique EmployeeCode constraint.** See §C, Phase 1. |
| Departments_D | KEEP | Branch-level department master | Fine. |
| Designations_DS | KEEP | Tenant-level designation master | Fine. |

### Attendance
| Table | Status | Purpose | Recommendation |
|---|---|---|---|
| AttendanceSessions_AS | KEEP | One taken-attendance event | Correct grouping by batch/subject/staff/date. |
| AttendanceRecords_AR | KEEP | Per-student status in a session | Correct 1:N from session, unique per (session, student). |

### Examinations
| Table | Status | Purpose | Recommendation |
|---|---|---|---|
| ExamTypes_ET | KEEP | Tenant exam-type master (unit test, final, etc.) | Fine. |
| Exams_EX | KEEP | An exam instance for a batch | Fine. |
| ExamSubjects_ES | KEEP | Subject-level marks config per exam | Correctly decoupled from CourseSubjects — allows per-exam max/pass marks. |
| ExamSchedules_ESC | KEEP | Date/time/room per exam subject | Fine, 1:1 with ExamSubjects. |
| Marks_M | KEEP | Per-student, per-subject marks | Correctly links to GradeScaleItems for letter grade. |
| Results_R | KEEP | Aggregated per-student exam result | Correct separation from subject-level Marks. |
| GradeScales_GS / GradeScaleItems_GSI | KEEP | Tenant-configurable grading systems | Good — supports multiple grading systems per tenant (percentage, GPA, etc.). |

### Fees & finance
| Table | Status | Purpose | Recommendation |
|---|---|---|---|
| FeeCategories_FC | KEEP | Tenant fee category master | Fine (tuition, transport, hostel, etc.). |
| FeeStructures_FS / FeeStructureItems_FSI | KEEP | Template fee schedule per course/batch/year | Fine. |
| StudentFeeAssignments_SFA | IMPROVE | Fee structure assigned to a student | Discount not traceable to a Discount record — see §C. |
| FeeInvoices_FI / FeeInvoiceItems_FII | IMPROVE | Actual billed invoice | Correctly decoupled from FeeStructure (preserves historical invoice values) — but add optional traceability FK — see §C. |
| Discounts_DIS | IMPROVE | Tenant discount master | **Currently orphaned — no table references it.** See §C. |
| PaymentMethods_PM | KEEP | Tenant payment method master | Fine. |
| Payments_PAY | KEEP | Payment received | Fine. |
| PaymentAllocations_PA | KEEP | Payment↔Invoice M:N | Correct design for partial/split payments. |
| Refunds_RF | KEEP | Refund against a payment | Fine for current scope; see §C for optional enhancement. |

### Operations
| Table | Status | Purpose | Recommendation |
|---|---|---|---|
| Vendors_V | KEEP | Vendor master | Fine. |
| ExpenseCategories_EC / Expenses_EXP | KEEP | Expense tracking | Fine. |
| Announcements_ANN | KEEP | Branch/tenant announcements | Fine. |
| Notifications_N / NotificationTemplates_NT | KEEP | Notification delivery + templates | Fine, generic multi-channel design. |
| ActivityLogs_ACL | KEEP | Free-text user activity feed | Distinct purpose from AuditLogs — keep both. |
| AuditLogs_AL | KEEP | Structured before/after entity change log | Distinct purpose from ActivityLogs — keep both. |

---

## B. Relationship Map (as-is)

```text
Organizations_O (tenant)
   │
   ├── OrganizationSettings_OS
   ├── Branches_B (campus)
   │      ├── Departments_D
   │      ├── Classrooms_CR
   │      └── Staff_ST *(orphaned — no enforced FK today)*
   │
   ├── AcademicYears_AY
   │      ├── Batches_BT ──────┐
   │      ├── Exams_EX         │
   │      └── FeeStructures_FS │
   │                           │
   ├── Programs_P (optional)   │
   │      └── Courses_C ───────┤
   │             ├── CourseSubjects_CS → Subjects_SB
   │             └── Batches_BT (Course + AcademicYear + Branch)
   │                    ├── BatchStudents_BS → Students_S
   │                    ├── AttendanceSessions_AS → AttendanceRecords_AR
   │                    ├── Timetables_TT
   │                    └── Exams_EX → ExamSubjects_ES → Marks_M
   │                                              └→ ExamSchedules_ESC
   │                                 └→ Results_R
   │
   ├── Students_S
   │      ├── Enrollments_E (Student+Year+Course+Batch)
   │      ├── StudentGuardians_SG → Guardians_G
   │      ├── StudentFeeAssignments_SFA → FeeStructures_FS
   │      ├── FeeInvoices_FI → FeeInvoiceItems_FII → FeeCategories_FC
   │      ├── Payments_PAY → PaymentAllocations_PA → FeeInvoices_FI
   │      ├── Refunds_RF → Payments_PAY
   │      └── EntityDocuments_ED → Documents_DOC / DocumentTypes_DT
   │
   ├── AdmissionApplications_AA → AdmissionApplicationDocuments_AAD
   │
   ├── GradeScales_GS → GradeScaleItems_GSI
   ├── Discounts_DIS *(orphaned — no enforced FK today)*
   ├── ExpenseCategories_EC / Vendors_V / PaymentMethods_PM → Expenses_EXP
   ├── CustomFields_CF → CustomFieldValues_CFV
   └── Announcements_ANN / Notifications_N / NotificationTemplates_NT / ActivityLogs_ACL / AuditLogs_AL
```

---

## C. Table-by-Table Changes

### 1. `Staff_ST` — missing keys (Critical)
**Current problem:** Unlike every other table in the script, `Staff_ST` has no `PRIMARY KEY` constraint on `ST_Id`, no `FOREIGN KEY` constraints to `Organizations_O`, `Branches_B`, `Departments_D`, or `Designations_DS`, and no `UNIQUE` constraint on `(ST_TenantId, ST_EmployeeCode)`.
**Recommended change (additive, non-destructive):**
```sql
ALTER TABLE dbo.Staff_ST ADD CONSTRAINT PK_Staff_ST_Id PRIMARY KEY CLUSTERED (ST_Id);

ALTER TABLE dbo.Staff_ST ADD CONSTRAINT UQ_Staff_ST_Org_EmployeeCode
    UNIQUE (ST_TenantId, ST_EmployeeCode);

ALTER TABLE dbo.Staff_ST WITH CHECK ADD CONSTRAINT FK_Staff_ST_TenantId
    FOREIGN KEY (ST_TenantId) REFERENCES dbo.Organizations_O (O_Id) ON DELETE CASCADE;

ALTER TABLE dbo.Staff_ST WITH CHECK ADD CONSTRAINT FK_Staff_ST_BranchId
    FOREIGN KEY (ST_BranchId) REFERENCES dbo.Branches_B (B_Id);

ALTER TABLE dbo.Staff_ST WITH CHECK ADD CONSTRAINT FK_Staff_ST_DepartmentId
    FOREIGN KEY (ST_DepartmentId) REFERENCES dbo.Departments_D (D_Id);

ALTER TABLE dbo.Staff_ST WITH CHECK ADD CONSTRAINT FK_Staff_ST_DesignationId
    FOREIGN KEY (ST_DesignationId) REFERENCES dbo.Designations_DS (DS_Id);
```
**Reason:** Right now nothing stops duplicate or orphaned staff rows, and no query can trust `ST_Id` as unique. This also blocks any future FK from Timetables/AttendanceSessions to Staff being validated.
**Impact:** Purely additive. Safe to run once existing data is confirmed clean (check for NULLs/dupes on ST_Id first).

### 2. `Discounts_DIS` — orphaned master table
**Current problem:** `Discounts_DIS` is defined and tenant-scoped but no other table has a foreign key to it. Discount amounts on `StudentFeeAssignments_SFA.SFA_DiscountAmount` and `FeeInvoiceItems_FII.FII_DiscountAmount` are just raw numbers with no link to *which* discount was applied, so you can't report "how many students used the Sibling Discount."
**Recommended change:**
```sql
ALTER TABLE dbo.StudentFeeAssignments_SFA ADD SFA_DiscountId UNIQUEIDENTIFIER NULL;
ALTER TABLE dbo.StudentFeeAssignments_SFA WITH CHECK ADD CONSTRAINT FK_StudentFeeAssignments_SFA_DiscountId
    FOREIGN KEY (SFA_DiscountId) REFERENCES dbo.Discounts_DIS (DIS_Id);

ALTER TABLE dbo.FeeInvoiceItems_FII ADD FII_DiscountId UNIQUEIDENTIFIER NULL;
ALTER TABLE dbo.FeeInvoiceItems_FII WITH CHECK ADD CONSTRAINT FK_FeeInvoiceItems_FII_DiscountId
    FOREIGN KEY (FII_DiscountId) REFERENCES dbo.Discounts_DIS (DIS_Id);
```
**Reason:** Nullable, additive columns — students who get a manual/negotiated discount with no matching Discount record simply leave it null. No existing data or query breaks.
**Impact:** None to existing rows; new capability only.

### 3. `Enrollments_E` vs `BatchStudents_BS` — overlapping responsibility
**Current problem:** Both tables track a student's association with a batch. `Enrollments_E` stores one `BatchId` per (student, year, course) with no history if the student changes batch/section mid-year. `BatchStudents_BS` stores `JoinedAt`/`LeftAt` history but has no `AcademicYearId`, so it can't distinguish "left Batch A in 2024" from "left Batch A in 2025" if a student re-joins the same batch code in a later cycle (unlikely given Batches are year-scoped, but worth naming explicitly).
**Recommended change:** Don't merge them — they answer different questions and both are needed. Document/enforce the intended division of labor:
- `Enrollments_E` = the **official, invoice/marks/transcript-relevant** record: "this student was enrolled in Course X for Academic Year Y." `E_BatchId` should be treated as *current section*, updated by the app when a student changes sections.
- `BatchStudents_BS` = the **section-membership history**: every join/leave event, including mid-year section transfers. This is what "history is preserved" queries (§10 of your brief) should read from — not `E_BatchId`.

Optionally, to make the history table more robust:
```sql
ALTER TABLE dbo.BatchStudents_BS ADD BS_Id UNIQUEIDENTIFIER NULL;
-- populate, then:
ALTER TABLE dbo.BatchStudents_BS ALTER COLUMN BS_Id UNIQUEIDENTIFIER NOT NULL;
ALTER TABLE dbo.BatchStudents_BS ADD CONSTRAINT DF_BatchStudents_BS_Id DEFAULT (newid()) FOR BS_Id;
-- then re-key: drop composite PK, add PK on BS_Id, keep (BatchId, StudentId, JoinedAt) as a unique index
```
This lets a student re-join the same batch (e.g., after a temporary leave) without violating the current composite PK, which today only allows one row per (Batch, Student) ever.
**Reason:** Prevents the current composite PK from silently blocking legitimate re-enrollment-in-same-batch scenarios.
**Impact:** Optional; only needed if re-joining the same batch is a real scenario for you. Classify as OPTIONAL, not required.

### 4. `CourseSubjects_CS` — not versioned by academic year
**Current problem:** Subject lists/marks per course (`CourseSubjects_CS`) are keyed only by `CourseId`. If curriculum changes next year (new subject added, marks changed), it silently applies retroactively to prior years' batches too, since there's no year dimension.
**Recommended change (optional, additive):**
```sql
ALTER TABLE dbo.CourseSubjects_CS ADD CS_AcademicYearId UNIQUEIDENTIFIER NULL;
ALTER TABLE dbo.CourseSubjects_CS WITH CHECK ADD CONSTRAINT FK_CourseSubjects_CS_AcademicYearId
    FOREIGN KEY (CS_AcademicYearId) REFERENCES dbo.AcademicYears_AY (AY_Id);
```
Leave it nullable: null = "applies to all years" (current behavior, unchanged for existing rows); populated = "applies only to this year," letting you version curriculum going forward without touching historical rows.
**Reason:** Preserves curriculum history requirement (§10) without forcing every existing row to be re-keyed.
**Impact:** Fully backward compatible — existing NULL rows behave exactly as today.

### 5. `FeeInvoices_FI` — no traceability to the fee structure that generated it
**Current problem:** `FeeInvoiceItems_FII` correctly references `FeeCategories_FC` (not `FeeStructureItems_FSI`), which is good — it means an invoice keeps its historical amounts even if the fee structure changes later. But there's no way to trace an invoice back to the `FeeStructures_FS`/`StudentFeeAssignments_SFA` that produced it.
**Recommended change (optional, additive):**
```sql
ALTER TABLE dbo.FeeInvoices_FI ADD FI_StudentFeeAssignmentId UNIQUEIDENTIFIER NULL;
ALTER TABLE dbo.FeeInvoices_FI WITH CHECK ADD CONSTRAINT FK_FeeInvoices_FI_StudentFeeAssignmentId
    FOREIGN KEY (FI_StudentFeeAssignmentId) REFERENCES dbo.StudentFeeAssignments_SFA (SFA_Id);
```
**Reason:** Improves auditability ("which assignment generated invoice #1234") without touching the intentional decoupling that protects historical invoice values.
**Impact:** Nullable, backward compatible.

### 6. `AdmissionApplicationDocuments_AAD` vs `EntityDocuments_ED`
**Observation, not a defect:** These look redundant at first glance (both link documents to an entity), but `AAD` carries admission-specific verification fields (`IsVerified`, `VerifiedBy`, `VerifiedAt`) that `ED` doesn't have. This is a legitimate specialization for the admissions workflow, not duplication.
**Recommendation:** KEEP both as-is. DEFER unifying them — doing so would mean adding verification columns to the generic `ED` table for a case that only applies to one entity type, which increases coupling for no real benefit.

---

## D. New Tables

Only one new table is genuinely justified by a gap the existing schema can't already cover.

### `Terms_TR` (optional — only if you bill/exam by semester, not just by year)
**Purpose:** Sub-division of an Academic Year for institutions that operate in semesters/terms (mainly higher-ed/college use case from your requirements list). Schools and coaching centres can simply not use it.
**Columns:**
| Column | Type | Notes |
|---|---|---|
| TR_Id | uniqueidentifier | PK |
| TR_TenantId | uniqueidentifier | FK → Organizations_O |
| TR_AcademicYearId | uniqueidentifier | FK → AcademicYears_AY |
| TR_Name | nvarchar(100) | e.g. "Semester 1" |
| TR_Code | nvarchar(50) | |
| TR_StartDate | date | |
| TR_EndDate | date | |
| TR_SequenceNo | int | ordering within the year |
| TR_CreatedAt / TR_UpdatedAt | datetime2(7) | |

**Primary key:** `TR_Id`
**Foreign keys:** `TR_TenantId → Organizations_O`, `TR_AcademicYearId → AcademicYears_AY`
**Unique constraint:** `(TR_AcademicYearId, TR_Code)`
**How it plugs in:** Add nullable `*_TermId` columns to `Batches_BT`, `Exams_EX`, and `FeeStructures_FS` only if/when semester-level granularity is actually needed. Until then, this table isn't needed — flag as **DEFER**, build only when a college customer requires semester-based billing/exams that a plain Academic Year can't express.

No other new tables are recommended. The schema already has the right generic primitives (Program/Course/Batch, CustomFields/CustomFieldValues, OrganizationSettings) to avoid needing institution-type-specific tables.

---

## E. Final Relationship Diagram (recommended state)

```text
Organizations_O
   │
   ├── OrganizationSettings_OS   (store "institution_type", feature flags, etc. here — no schema change needed)
   ├── Branches_B
   │      ├── Departments_D
   │      ├── Classrooms_CR
   │      └── Staff_ST  ← [FIX: add PK + FKs]
   │
   ├── AcademicYears_AY
   │      └── (Terms_TR)  ← optional, only if semester billing/exams needed
   │
   ├── Programs_P (optional)
   │      └── Courses_C
   │             ├── CourseSubjects_CS  ← [add optional AY-scoping for curriculum versioning]
   │             └── Batches_BT
   │                    ├── BatchStudents_BS   (section-membership HISTORY, source of truth for "current section")
   │                    ├── AttendanceSessions_AS → AttendanceRecords_AR
   │                    ├── Timetables_TT
   │                    └── Exams_EX → ExamSubjects_ES → { Marks_M, ExamSchedules_ESC }
   │                                              └→ Results_R
   │
   ├── Students_S
   │      ├── Enrollments_E   (official course/year record — CURRENT batch, not history)
   │      ├── StudentGuardians_SG → Guardians_G
   │      ├── StudentFeeAssignments_SFA → FeeStructures_FS
   │      │        └→ Discounts_DIS  ← [NEW FK]
   │      ├── FeeInvoices_FI ← [NEW optional FK to StudentFeeAssignments_SFA]
   │      │        └── FeeInvoiceItems_FII → FeeCategories_FC
   │      │                 └→ Discounts_DIS  ← [NEW FK]
   │      ├── Payments_PAY → PaymentAllocations_PA → FeeInvoices_FI
   │      ├── Refunds_RF → Payments_PAY
   │      └── EntityDocuments_ED → Documents_DOC / DocumentTypes_DT
   │
   ├── AdmissionApplications_AA → AdmissionApplicationDocuments_AAD
   ├── GradeScales_GS → GradeScaleItems_GSI
   ├── Discounts_DIS  ← [now referenced]
   └── CustomFields_CF → CustomFieldValues_CFV
```

---

## F. Migration Priority

**Phase 1 — Safe / Essential**
1. Add `PRIMARY KEY`, `UNIQUE (TenantId, EmployeeCode)`, and all four `FOREIGN KEY` constraints to `Staff_ST`.
   *(Run a duplicate/orphan check on `ST_Id` and `(ST_TenantId, ST_EmployeeCode)` first, in case bad data already exists.)*

**Phase 2 — Improvement**
2. Add `SFA_DiscountId` and `FII_DiscountId` nullable FKs to `Discounts_DIS`.
3. Add nullable `FI_StudentFeeAssignmentId` FK on `FeeInvoices_FI` for traceability.
4. Add nullable `CS_AcademicYearId` FK on `CourseSubjects_CS` for curriculum versioning going forward.
5. Document (or enforce via app logic) the intended division between `Enrollments_E` (current state) and `BatchStudents_BS` (history) so both stay in sync.

**Phase 3 — Optional Future Enhancement**
6. Re-key `BatchStudents_BS` to a surrogate `BS_Id` PK if repeat-joins to the same batch become a real scenario.
7. Add `Terms_TR` only if/when a customer needs true semester-level billing or exams (colleges). Not needed for schools or coaching centres.
8. Consider a dedicated "class teacher" / staff-role-on-batch table if you need more than the subject-teacher assignments `Timetables_TT` already gives you.

---

## What NOT to change

To be explicit, since the brief asks for this: the following are **already correct** and should not be touched —
- The Organization → Branch → Program(optional) → Course → Batch chain (this *is* your generic institution model).
- `ExamSubjects_ES` being decoupled from `CourseSubjects_CS` (protects exam configs from curriculum edits).
- `FeeInvoiceItems_FII` referencing `FeeCategories_FC` directly rather than `FeeStructureItems_FSI` (protects historical invoices from fee-structure edits).
- `Timetables_TT`'s `EffectiveFrom`/`EffectiveTo` pattern (correct historical modeling — reuse this pattern anywhere else you need versioned assignments).
- `Guardians_G` being tenant-scoped rather than branch-scoped.
- `OrganizationSettings_OS` and `CustomFields_CF`/`CustomFieldValues_CFV` as the extension mechanisms for institution-specific needs — resist the urge to add institution-type-specific columns anywhere else in the schema.