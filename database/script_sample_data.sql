USE [IMS]
GO

-- =========================================================================================
-- Sample Data: 5 records per master table with proper cascading dependencies
-- TenantId is a fixed GUID (simulates JWT tenant)
-- =========================================================================================

DECLARE @TenantId UNIQUEIDENTIFIER = '11111111-1111-1111-1111-111111111111';

-- =========================================================================================
-- 1. Branches_B (no FK dependencies)
-- =========================================================================================
INSERT INTO dbo.Branches_B (B_Id, B_TenantId, B_Name, B_Code, B_Email, B_Phone, B_AddressLine1, B_AddressLine2, B_City, B_State, B_PostalCode, B_CountryCode, B_Status)
VALUES
('22222222-2222-2222-2222-222222222201', @TenantId, 'Main Campus',        'BR-001', 'main@ims.com',        '9000000001', '123 College Road',   'Near Central Park',   'Mumbai',    'Maharashtra', '400001', 'IN', 'active'),
('22222222-2222-2222-2222-222222222202', @TenantId, 'North Branch',        'BR-002', 'north@ims.com',       '9000000002', '456 School Street',  'Opp. City Mall',      'Delhi',     'Delhi',       '110001', 'IN', 'active'),
('22222222-2222-2222-2222-222222222203', @TenantId, 'South Branch',        'BR-003', 'south@ims.com',       '9000000003', '789 Academy Lane',   'Near Bus Stand',      'Chennai',   'Tamil Nadu',  '600001', 'IN', 'active'),
('22222222-2222-2222-2222-222222222204', @TenantId, 'East Wing',           'BR-004', 'east@ims.com',        '9000000004', '321 Learning Ave',   'Behind Library',      'Kolkata',   'West Bengal', '700001', 'IN', 'active'),
('22222222-2222-2222-2222-222222222205', @TenantId, 'West Campus',         'BR-005', 'west@ims.com',        '9000000005', '654 Education Blvd', 'Next to Stadium',     'Pune',      'Maharashtra', '411001', 'IN', 'active');
GO

-- =========================================================================================
-- 2. AcademicYears_AY (no FK dependencies)
-- =========================================================================================
INSERT INTO dbo.AcademicYears_AY (AY_Id, AY_TenantId, AY_Name, AY_Code, AY_StartDate, AY_EndDate, AY_IsCurrent)
VALUES
('33333333-3333-3333-3333-333333333301', @TenantId, '2023-2024', 'AY-2023', '2023-04-01', '2024-03-31', 0),
('33333333-3333-3333-3333-333333333302', @TenantId, '2024-2025', 'AY-2024', '2024-04-01', '2025-03-31', 0),
('33333333-3333-3333-3333-333333333303', @TenantId, '2025-2026', 'AY-2025', '2025-04-01', '2026-03-31', 1),
('33333333-3333-3333-3333-333333333304', @TenantId, '2026-2027', 'AY-2026', '2026-04-01', '2027-03-31', 0),
('33333333-3333-3333-3333-333333333305', @TenantId, '2027-2028', 'AY-2027', '2027-04-01', '2028-03-31', 0);
GO

-- =========================================================================================
-- 3. Programs_P (no FK dependencies)
-- =========================================================================================
INSERT INTO dbo.Programs_P (P_Id, P_TenantId, P_Name, P_Code, P_DurationValue, P_DurationUnit, P_Description, P_Status)
VALUES
('44444444-4444-4444-4444-444444444401', @TenantId, 'Bachelor of Science',         'P-BSC',   3, 'Years', 'Undergraduate science program',           'active'),
('44444444-4444-4444-4444-444444444402', @TenantId, 'Bachelor of Arts',             'P-BA',    3, 'Years', 'Undergraduate arts program',               'active'),
('44444444-4444-4444-4444-444444444403', @TenantId, 'Master of Science',            'P-MSC',   2, 'Years', 'Postgraduate science program',             'active'),
('44444444-4444-4444-4444-444444444404', @TenantId, 'Diploma in Computer Science',  'P-DCS',   1, 'Years', 'One-year diploma program',                 'active'),
('44444444-4444-4444-4444-444444444405', @TenantId, 'Higher Secondary',             'P-HSC',   2, 'Years', '11th and 12th grade program',              'active');
GO

-- =========================================================================================
-- 4. Departments_D (depends on: Branches_B)
-- =========================================================================================
INSERT INTO dbo.Departments_D (D_Id, D_TenantId, D_BranchId, D_Name, D_Code, D_Description)
VALUES
('55555555-5555-5555-5555-555555555501', @TenantId, '22222222-2222-2222-2222-222222222201', 'Computer Science',  'DEPT-CS',  'CS and IT department'),
('55555555-5555-5555-5555-555555555502', @TenantId, '22222222-2222-2222-2222-222222222201', 'Mathematics',       'DEPT-MATH','Mathematics department'),
('55555555-5555-5555-5555-555555555503', @TenantId, '22222222-2222-2222-2222-222222222202', 'Physics',           'DEPT-PHYS','Physics department'),
('55555555-5555-5555-5555-555555555504', @TenantId, '22222222-2222-2222-2222-222222222203', 'Chemistry',         'DEPT-CHEM','Chemistry department'),
('55555555-5555-5555-5555-555555555505', @TenantId, '22222222-2222-2222-2222-222222222201', 'Administration',    'DEPT-ADM', 'Administrative department');
GO

-- =========================================================================================
-- 5. Designations_DS (no FK dependencies)
-- =========================================================================================
INSERT INTO dbo.Designations_DS (DS_Id, DS_TenantId, DS_Name, DS_Code)
VALUES
('66666666-6666-6666-6666-666666666601', @TenantId, 'Professor',            'DS-PROF'),
('66666666-6666-6666-6666-666666666602', @TenantId, 'Associate Professor',  'DS-APROF'),
('66666666-6666-6666-6666-666666666603', @TenantId, 'Assistant Professor',  'DS-ASST'),
('66666666-6666-6666-6666-666666666604', @TenantId, 'Lecturer',             'DS-LECT'),
('66666666-6666-6666-6666-666666666605', @TenantId, 'Lab Assistant',        'DS-LAB');
GO

-- =========================================================================================
-- 6. Courses_C (depends on: Programs_P)
-- =========================================================================================
INSERT INTO dbo.Courses_C (C_Id, C_TenantId, C_ProgramId, C_Name, C_Code, C_Description, C_Status)
VALUES
('77777777-7777-7777-7777-777777777701', @TenantId, '44444444-4444-4444-4444-444444444401', 'B.Sc. Computer Science',  'C-BSC-CS',  'Bachelor of Science in Computer Science',  'active'),
('77777777-7777-7777-7777-777777777702', @TenantId, '44444444-4444-4444-4444-444444444401', 'B.Sc. Mathematics',       'C-BSC-MATH','Bachelor of Science in Mathematics',       'active'),
('77777777-7777-7777-7777-777777777703', @TenantId, '44444444-4444-4444-4444-444444444402', 'B.A. English',            'C-BA-ENG',  'Bachelor of Arts in English',              'active'),
('77777777-7777-7777-7777-777777777704', @TenantId, '44444444-4444-4444-4444-444444444403', 'M.Sc. Computer Science',  'C-MSC-CS',  'Master of Science in Computer Science',    'active'),
('77777777-7777-7777-7777-777777777705', @TenantId, '44444444-4444-4444-4444-444444444404', 'Diploma in Web Dev',      'C-DCS-WEB', 'Diploma in Web Development',               'active');
GO

-- =========================================================================================
-- 7. Subjects_SB (no FK dependencies)
-- =========================================================================================
INSERT INTO dbo.Subjects_SB (SB_Id, SB_TenantId, SB_Name, SB_Code, SB_Description, SB_Credits, SB_MaxMarks, SB_PassMarks)
VALUES
('88888888-8888-8888-8888-888888888801', @TenantId, 'Mathematics',     'SB-MATH', 'Core mathematics subject',   4, 100, 40),
('88888888-8888-8888-8888-888888888802', @TenantId, 'Physics',         'SB-PHYS', 'Classical and modern physics', 4, 100, 40),
('88888888-8888-8888-8888-888888888803', @TenantId, 'Computer Science','SB-CS',   'Programming and algorithms',  4, 100, 40),
('88888888-8888-8888-8888-888888888804', @TenantId, 'English',         'SB-ENG',  'Language and literature',      3, 100, 40),
('88888888-8888-8888-8888-888888888805', @TenantId, 'Chemistry',       'SB-CHEM', 'General chemistry',            4, 100, 40);
GO

-- =========================================================================================
-- 8. DocumentTypes_DT (no FK dependencies)
-- =========================================================================================
INSERT INTO dbo.DocumentTypes_DT (DT_Id, DT_TenantId, DT_Name, DT_Code, DT_EntityType, DT_IsRequired)
VALUES
('99999999-9999-9999-9999-999999999901', @TenantId, 'Admission Form',      'DT-ADM',   'student',  1),
('99999999-9999-9999-9999-999999999902', @TenantId, 'Marksheet',           'DT-MARK',  'student',  0),
('99999999-9999-9999-9999-999999999903', @TenantId, 'ID Proof',            'DT-ID',    'student',  1),
('99999999-9999-9999-9999-999999999904', @TenantId, 'Transfer Certificate','DT-TC',    'student',  0),
('99999999-9999-9999-9999-999999999905', @TenantId, 'Staff Appointment',   'DT-STAFF', 'staff',    1);
GO

-- =========================================================================================
-- 9. ExamTypes_ET (no FK dependencies)
-- =========================================================================================
INSERT INTO dbo.ExamTypes_ET (ET_Id, ET_TenantId, ET_Name, ET_Code, ET_WeightagePercentage)
VALUES
('AAAA1111-1111-1111-1111-111111111101', @TenantId, 'Unit Test 1',       'ET-UT1',  10.00),
('AAAA1111-1111-1111-1111-111111111102', @TenantId, 'Unit Test 2',       'ET-UT2',  10.00),
('AAAA1111-1111-1111-1111-111111111103', @TenantId, 'Mid-Term Exam',     'ET-MID',  30.00),
('AAAA1111-1111-1111-1111-111111111104', @TenantId, 'Pre-Final Exam',    'ET-PREL', 20.00),
('AAAA1111-1111-1111-1111-111111111105', @TenantId, 'Final Exam',        'ET-FINAL',30.00);
GO

-- =========================================================================================
-- 10. ExpenseCategories_EC (no FK dependencies)
-- =========================================================================================
INSERT INTO dbo.ExpenseCategories_EC (EC_Id, EC_TenantId, EC_Name, EC_Code, EC_Description)
VALUES
('AAAA2222-2222-2222-2222-222222222201', @TenantId, 'Staff Salary',      'EC-SAL',   'Monthly staff salary payments'),
('AAAA2222-2222-2222-2222-222222222202', @TenantId, 'Electricity Bill',   'EC-ELEC',  'Monthly electricity charges'),
('AAAA2222-2222-2222-2222-222222222203', @TenantId, 'Lab Equipment',      'EC-LAB',   'Science lab equipment purchase'),
('AAAA2222-2222-2222-2222-222222222204', @TenantId, 'Stationery',         'EC-STAT',  'Pens, papers, and office supplies'),
('AAAA2222-2222-2222-2222-222222222205', @TenantId, 'Maintenance',        'EC-MAINT', 'Building and furniture maintenance');
GO

-- =========================================================================================
-- 11. FeeCategories_FC (no FK dependencies)
-- =========================================================================================
INSERT INTO dbo.FeeCategories_FC (FC_Id, FC_TenantId, FC_Name, FC_Code, FC_Description, FC_IsRefundable)
VALUES
('AAAA3333-3333-3333-3333-333333333301', @TenantId, 'Tuition Fee',       'FC-TUITION',  'Regular tuition fees',         0),
('AAAA3333-3333-3333-3333-333333333302', @TenantId, 'Library Fee',       'FC-LIBRARY',  'Library access charges',        0),
('AAAA3333-3333-3333-3333-333333333303', @TenantId, 'Lab Fee',           'FC-LAB',      'Laboratory usage charges',      0),
('AAAA3333-3333-3333-3333-333333333304', @TenantId, 'Hostel Fee',        'FC-HOSTEL',   'Hostel accommodation fee',      1),
('AAAA3333-3333-3333-3333-333333333305', @TenantId, 'Exam Fee',          'FC-EXAM',     'Examination charges',           0);
GO

-- =========================================================================================
-- 12. GradeScales_GS (no FK dependencies)
-- =========================================================================================
INSERT INTO dbo.GradeScales_GS (GS_Id, GS_TenantId, GS_Name, GS_Code, GS_Description, GS_IsDefault)
VALUES
('AAAA4444-4444-4444-4444-444444444401', @TenantId, 'Percentage Scale',  'GS-PCT',  'Standard percentage grading',       1),
('AAAA4444-4444-4444-4444-444444444402', @TenantId, 'GPA Scale (10)',    'GS-GPA10','GPA on 10-point scale',             0),
('AAAA4444-4444-4444-4444-444444444403', @TenantId, 'GPA Scale (4)',     'GS-GPA4', 'GPA on 4-point scale (US style)',    0),
('AAAA4444-4444-4444-4444-444444444404', @TenantId, 'Letter Grade',      'GS-LET',  'A/B/C/D/F letter grading',          0),
('AAAA4444-4444-4444-4444-444444444405', @TenantId, 'Distinction Scale', 'GS-DIST', 'Pass/Fail with distinction tiers',  0);
GO

-- =========================================================================================
-- 13. Classrooms_CR (depends on: Branches_B)
-- =========================================================================================
INSERT INTO dbo.Classrooms_CR (CR_Id, CR_TenantId, CR_BranchId, CR_Name, CR_Code, CR_Capacity, CR_Location)
VALUES
('AAAA5555-5555-5555-5555-555555555501', @TenantId, '22222222-2222-2222-2222-222222222201', 'Room 101',  'CR-101', 60, 'Block A, Ground Floor'),
('AAAA5555-5555-5555-5555-555555555502', @TenantId, '22222222-2222-2222-2222-222222222201', 'Room 102',  'CR-102', 60, 'Block A, Ground Floor'),
('AAAA5555-5555-5555-5555-555555555503', @TenantId, '22222222-2222-2222-2222-222222222202', 'Hall A',    'CR-HA',  200, 'North Campus, Main Building'),
('AAAA5555-5555-5555-5555-555555555504', @TenantId, '22222222-2222-2222-2222-222222222203', 'Lab 201',   'CR-L201',40, 'South Branch, Lab Block'),
('AAAA5555-5555-5555-5555-555555555505', @TenantId, '22222222-2222-2222-2222-222222222201', 'Seminar Hall','CR-SH', 150, 'Block B, First Floor');
GO

-- =========================================================================================
-- 14. PaymentMethods_PM (no FK dependencies)
-- =========================================================================================
INSERT INTO dbo.PaymentMethods_PM (PM_Id, PM_TenantId, PM_Name, PM_Type, PM_IsActive)
VALUES
('AAAA6666-6666-6666-6666-666666666601', @TenantId, 'Cash',            'cash',     1),
('AAAA6666-6666-6666-6666-666666666602', @TenantId, 'UPI',             'online',   1),
('AAAA6666-6666-6666-6666-666666666603', @TenantId, 'Bank Transfer',   'bank',     1),
('AAAA6666-6666-6666-6666-666666666604', @TenantId, 'Credit Card',     'card',     1),
('AAAA6666-6666-6666-6666-666666666605', @TenantId, 'Debit Card',      'card',     1);
GO

-- =========================================================================================
-- 15. Discounts_DIS (no FK dependencies)
-- =========================================================================================
INSERT INTO dbo.Discounts_DIS (DIS_Id, DIS_TenantId, DIS_Name, DIS_Code, DIS_DiscountType, DIS_Value, DIS_Description, DIS_IsActive)
VALUES
('AAAA7777-7777-7777-7777-777777777701', @TenantId, 'Sibling Discount',     'DIS-SIB',  'Percentage', 10.00, 'Discount for second child from same family', 1),
('AAAA7777-7777-7777-7777-777777777702', @TenantId, 'Early Bird Discount',  'DIS-EBD',  'Percentage', 5.00,  'Fee paid before due date',                    1),
('AAAA7777-7777-7777-7777-777777777703', @TenantId, 'Staff Ward Discount',  'DIS-SWD',  'Percentage', 25.00,'Discount for children of staff members',       1),
('AAAA7777-7777-7777-7777-777777777704', @TenantId, 'Merit Scholarship',    'DIS-MER',  'Fixed',      5000.00,'Scholarship for top scorers',               1),
('AAAA7777-7777-7777-7777-777777777705', @TenantId, 'Government Subsidy',   'DIS-GOV',  'Percentage', 15.00,'Government scheme discount',                   1);
GO

-- =========================================================================================
-- 16. Vendors_V (no FK dependencies)
-- =========================================================================================
INSERT INTO dbo.Vendors_V (V_Id, V_TenantId, V_Name, V_Code, V_Email, V_Phone, V_TaxNumber, V_Address)
VALUES
('AAAA8888-8888-8888-8888-888888888801', @TenantId, 'PrintWorld',           'VEN-PRT', 'info@printworld.com',   '9110000001', 'GSTIN27AABCU9603R1ZM', '12 Printing Press Lane, Mumbai'),
('AAAA8888-8888-8888-8888-888888888802', @TenantId, 'TechSupply India',     'VEN-TECH','sales@techsupply.in',   '9110000002', 'GSTIN07AACCT1234F1ZP', '45 IT Park Road, Delhi'),
('AAAA8888-8888-8888-8888-888888888803', @TenantId, 'FurniturePlus',        'VEN-FUR', 'contact@furnplus.com',  '9110000003', 'GSTIN33BBBFU5678G1ZQ', '78 Industrial Area, Chennai'),
('AAAA8888-8888-8888-8888-888888888804', @TenantId, 'CleanFresh Services',  'VEN-CLN', 'hello@cleanfresh.in',   '9110000004', 'GSTIN19CCCFG9012H1ZR', '90 Service Block, Kolkata'),
('AAAA8888-8888-8888-8888-888888888805', @TenantId, 'EduBooks Publishers',  'VEN-EDU', 'orders@edubooks.in',    '9110000005', 'GSTIN24DDD EB3456I1ZS', '23 Book Market, Pune');
GO

-- =========================================================================================
-- 17. NotificationTemplates_NT (no FK dependencies)
-- =========================================================================================
INSERT INTO dbo.NotificationTemplates_NT (NT_Id, NT_TenantId, NT_Name, NT_EventKey, NT_Channel, NT_Subject, NT_BodyTemplate, NT_IsActive)
VALUES
('AAAA9999-9999-9999-9999-999999999901', @TenantId, 'Fee Receipt',           'NT-FEE',     'Email', 'Fee Payment Receipt',         'Dear {StudentName}, your fee payment of {Amount} has been received. Receipt #{ReceiptNo}.', 1),
('AAAA9999-9999-9999-9999-999999999902', @TenantId, 'Exam Schedule',         'NT-EXAM',    'Email', 'Upcoming Exam Schedule',     'Dear {StudentName}, your {ExamName} is scheduled on {ExamDate}. Please prepare well.', 1),
('AAAA9999-9999-9999-9999-999999999903', @TenantId, 'Attendance Alert',      'NT-ATT',     'SMS',   'Attendance Alert',           'Dear Parent, {StudentName} was marked absent on {Date}.', 1),
('AAAA9999-9999-9999-9999-999999999904', @TenantId, 'Admission Confirmation','NT-ADM',     'Email', 'Admission Confirmed',        'Dear {StudentName}, your admission to {CourseName} has been confirmed. Welcome aboard!', 1),
('AAAA9999-9999-9999-9999-999999999905', @TenantId, 'Result Published',      'NT-RESULT',  'Email', 'Exam Results Published',     'Dear {StudentName}, your {ExamName} results are now available. Percentage: {Percentage}%.', 1);
GO

PRINT 'All sample data inserted successfully (5 records per table, 17 tables).';
GO
