--Creating Galam Tables
create table tblTopGroups
(
topGroupId int primary key,
topGroupName nvarchar(20) not null,
)

create table tblProductGroups
(
productGroupId int primary key,
productGroupName nvarchar(20) not null,
topGroupId int references tblTopGroups(topGroupId) not null
)

create table tblProductFamilies
(
productFamilyId varchar(50) primary key,
productFamilyName nvarchar(20) not null,
productGroupId int references tblProductGroups(productGroupId) not null
)

create table tblFrProductFamilies
(
frProductFamilyId varchar(50) primary key references tblProductFamilies(productFamilyId),
coefficientS float not null,
coefficientN float not null,
coefficientD float not null,
coefficientE float not null,
coefficientOF float not null,
coefficientL float not null,
frProductFamilyDistribution float
)

create table tblItems
(
itemNumber nvarchar(50) primary key,
itemDescription nvarchar(100) not null,
productFamilyId varchar(50) references tblProductFamilies(productFamilyId) not null
)

create table tblPeriods
(
yearP int,
quarterP int,
dailyProduction int,
greensUsing int,
factorS int,
factorN int,
factorD int,
factorE int,
factorOF int,
factorL int,
storageCapacity int,
conteinersInventory int,

primary key(yearP,quarterP)
)

create table tblWeeks
(
yearP int,
quarterP int,
weekNumber int,
actualProduction float,
actualGreens float,
actualSales float,
actualInventory float,
actualInventoryS float,
actualInventoryN float,
actualInventoryD float,
actualInventoryE float,
actualInventoryOF float,
actualInventoryExternal float,

foreign key(yearP,quarterP) references tblPeriods(yearP,quarterP),
primary key(yearP,quarterP,weekNumber)
)

create table tblPeople
(
personId int primary key,
personFullName nvarchar(50),
personEmail varchar(50)
)

create table tblSaleMenager
(
personId int primary key references tblPeople(personId),
isActiveSaleMenager bit
)

create table tblCountries
(
countryId char(2) primary key,
countryName varchar(50),
countryMarket varchar(50)
)

create table tblCustomers
(
customerNumber int primary key,
customerName nvarchar(50),
isLocal bit not null,
saleMenagerId int references tblSaleMenager(personId) not null
)

create table tblShipTos
(
customerNumber int references tblCustomers(customerNumber),
shipToName nvarchar(50),
countryId char(2) references tblCountries(countryId) not null,
shipToActive bit not null,
shipToIsEndCustomer bit not null,

primary key(customerNumber,shipToName)
)

create table tblUsers
(
userName varchar(50) primary key,
personId int references tblPeople(personId),
userPasswordHash binary,
userPasswordSalt binary,
userActive bit,
userIsNewPassword bit,
userPassword varchar(50) /*temp*/
)

create table tblUserTypes
(
userTypeId int primary key,
userTypeName nvarchar(50) not null,
)

create table tblUsersVsUserTypes
(
userName varchar(50) references tblUsers(userName),
userTypeId int references tblUserTypes(userTypeId),

primary key(userName,userTypeId)
)

create table tblPermissionTypes
(
permissionTypeId int primary key,
permissionTypeName nvarchar(50) not null,
)

create table tblUsersVsPermissionTypes
(
userName varchar(50) references tblUsers(userName),
permissionTypeId int references tblPermissionTypes(permissionTypeId),

primary key(userName,permissionTypeId)
)

create table tblUserTypesVsPermissionTypes
(
userTypeId int references tblUserTypes(userTypeId),
permissionTypeId int references tblPermissionTypes(permissionTypeId),

primary key(userTypeId,permissionTypeId)
)

create table tblSales
(
itemNumber nvarchar(50) references tblItems(itemNumber),
yearP int,
quarterP int,
customerNumber int references tblCustomers(customerNumber),
salesQty float not null,

foreign key(yearP,quarterP) references tblPeriods(yearP,quarterP),
primary key(itemNumber,yearP,quarterP,customerNumber)
)

create table tblForecastTransactions
(
forecastTransactionsId int IDENTITY (1, 1) primary key,
itemNumber nvarchar(50) references tblItems(itemNumber) not null,
yearP int not null,
quarterP int not null,
customerNumber int not null,
shipToName nvarchar(50) not null,
forecastTransactionCreatedBy varchar(50) references tblUsers(userName) not null,
forecastTransactionLastUpdateBy varchar(50) references tblUsers(userName) not null,
forecastTransactionCreationDate datetime2 not null,
forecastTransactionQty float not null,
forecastTransactionStatus varchar(50) not null,
forecastTransactionLastUpdateDate datetime2 not null,
forecastTransactionIsMelting bit not null,

foreign key(yearP,quarterP) references tblPeriods(yearP,quarterP),
foreign key(customerNumber,shipToName) references tblShipTos(customerNumber,shipToName),
)

--Add User Types
insert into tblUserTypes (userTypeId, userTypeName) values (1, 'מנהל תפי')
insert into tblUserTypes (userTypeId, userTypeName) values (2, 'מנהל קטגוריה פרוקטוז')
insert into tblUserTypes (userTypeId, userTypeName) values (3, 'מנהל שוק מקומי')
insert into tblUserTypes (userTypeId, userTypeName) values (4, 'אדמיניסטרטור')

--Add Permission Types
insert into tblPermissionTypes (permissionTypeId, permissionTypeName) values (1, 'עדכון תחזית Abroad')
insert into tblPermissionTypes (permissionTypeId, permissionTypeName) values (2, 'עדכון תחזית Local')
insert into tblPermissionTypes (permissionTypeId, permissionTypeName) values (3, 'אישור בקשות Abroad')
insert into tblPermissionTypes (permissionTypeId, permissionTypeName) values (4, 'אישור בקשות Local')
insert into tblPermissionTypes (permissionTypeId, permissionTypeName) values (5, 'עדכון נתוני ייצור')
insert into tblPermissionTypes (permissionTypeId, permissionTypeName) values (6, 'ניהול לקוחות קצה')
insert into tblPermissionTypes (permissionTypeId, permissionTypeName) values (7, 'צפיה בתחזית סוויסוית')

--Add Permissions of User Types
insert into tblUserTypesVsPermissionTypes (userTypeId, permissionTypeId) values (1, 5)
insert into tblUserTypesVsPermissionTypes (userTypeId, permissionTypeId) values (1, 7)
insert into tblUserTypesVsPermissionTypes (userTypeId, permissionTypeId) values (2, 1)
insert into tblUserTypesVsPermissionTypes (userTypeId, permissionTypeId) values (2, 2)
insert into tblUserTypesVsPermissionTypes (userTypeId, permissionTypeId) values (2, 3)
insert into tblUserTypesVsPermissionTypes (userTypeId, permissionTypeId) values (2, 4)
insert into tblUserTypesVsPermissionTypes (userTypeId, permissionTypeId) values (2, 6)
insert into tblUserTypesVsPermissionTypes (userTypeId, permissionTypeId) values (3, 2)
insert into tblUserTypesVsPermissionTypes (userTypeId, permissionTypeId) values (3, 4)

--Add Top Groups
insert into tblTopGroups (topGroupId, topGroupName) values (61601, 'Fructose')

--Add Product Groups
insert into tblProductGroups (productGroupId, productGroupName, topGroupId) values (142542, 'Crystelline Fructose', 61601)
insert into tblProductGroups (productGroupId, productGroupName, topGroupId) values (142554, 'Liquid Fructose', 61601)

--Add Product Families
insert into tblProductFamilies (ProductFamilyId, ProductFamilyName, productGroupId) values ('50317', 'Fructose A', 142542)
insert into tblProductFamilies (ProductFamilyId, ProductFamilyName, productGroupId) values ('50311', 'Fructose D', 142542)
insert into tblProductFamilies (ProductFamilyId, ProductFamilyName, productGroupId) values ('50316', 'Fructose E', 142542)
insert into tblProductFamilies (ProductFamilyId, ProductFamilyName, productGroupId) values ('50319', 'Fructose MS', 142542)
insert into tblProductFamilies (ProductFamilyId, ProductFamilyName, productGroupId) values ('50313', 'Fructose N', 142542)
insert into tblProductFamilies (ProductFamilyId, ProductFamilyName, productGroupId) values ('50320', 'Fructose NP', 142542)
insert into tblProductFamilies (ProductFamilyId, ProductFamilyName, productGroupId) values ('50318', 'Fructose OF', 142542)
insert into tblProductFamilies (ProductFamilyId, ProductFamilyName, productGroupId) values ('50314', 'Fructose S', 142542)
insert into tblProductFamilies (ProductFamilyId, ProductFamilyName, productGroupId) values ('50411', 'Liquid Fructose', 142554)

--Add Fructose Product Families
insert into tblFrProductFamilies (frProductFamilyId, coefficientS, coefficientN, coefficientD, coefficientE, coefficientOF, coefficientL, frProductFamilyDistribution) values ('50317', 0.81, 0, 0.19, 0, 0, 0, 0)
insert into tblFrProductFamilies (frProductFamilyId, coefficientS, coefficientN, coefficientD, coefficientE, coefficientOF, coefficientL, frProductFamilyDistribution) values ('50311', 0, 0, 1, 0, 0, 0, 0.15)
insert into tblFrProductFamilies (frProductFamilyId, coefficientS, coefficientN, coefficientD, coefficientE, coefficientOF, coefficientL, frProductFamilyDistribution) values ('50316', 0, 0, 0, 1, 0, 0, 0.1)
insert into tblFrProductFamilies (frProductFamilyId, coefficientS, coefficientN, coefficientD, coefficientE, coefficientOF, coefficientL, frProductFamilyDistribution) values ('50319', 1.2, -0.2, 0, 0, 0, 0, 0)
insert into tblFrProductFamilies (frProductFamilyId, coefficientS, coefficientN, coefficientD, coefficientE, coefficientOF, coefficientL, frProductFamilyDistribution) values ('50313', 0, 1, 0, 0, 0, 0, 0.22)
insert into tblFrProductFamilies (frProductFamilyId, coefficientS, coefficientN, coefficientD, coefficientE, coefficientOF, coefficientL, frProductFamilyDistribution) values ('50320', 1, 0, 0, 0, 0, 0, 0)
insert into tblFrProductFamilies (frProductFamilyId, coefficientS, coefficientN, coefficientD, coefficientE, coefficientOF, coefficientL, frProductFamilyDistribution) values ('50318', 0, 0, 0, 0, 1, 0, 0.02)
insert into tblFrProductFamilies (frProductFamilyId, coefficientS, coefficientN, coefficientD, coefficientE, coefficientOF, coefficientL, frProductFamilyDistribution) values ('50314', 1, 0, 0, 0, 0, 0, 0.51)
insert into tblFrProductFamilies (frProductFamilyId, coefficientS, coefficientN, coefficientD, coefficientE, coefficientOF, coefficientL, frProductFamilyDistribution) values ('50411', 0, 0, 0, 0, 0, 1, 0)

--Add System Users
insert into tblUsers (userName, userActive) values ('SYSTEM', 0)

--Add Temp Users
insert into tblUsers (userName, userActive, userIsNewPassword, userPassword) values ('TAPI', 1, 0, 'Ab123456')
insert into tblUsersVsUserTypes (userName, userTypeId) values ('TAPI', 1)

insert into tblUsers (userName, userActive, userIsNewPassword, userPassword) values ('ABROADM', 1, 0, 'Ab123456')
insert into tblUsersVsUserTypes (userName, userTypeId) values ('ABROADM', 2)

insert into tblUsers (userName, userActive, userIsNewPassword, userPassword) values ('LOCALM', 1, 0, 'Ab123456')
insert into tblUsersVsUserTypes (userName, userTypeId) values ('LOCALM', 3)

insert into tblUsers (userName, userActive, userIsNewPassword, userPassword) values ('ADMIN', 1, 0, 'Ab123456')
insert into tblUsersVsUserTypes (userName, userTypeId) values ('ADMIN', 4)

insert into tblPeople (personId, personFullName, personEmail) values (1, 'מנהל מכירות 0', 'stam@stam.com')
insert into tblSaleMenager (personId, isActiveSaleMenager) values (1, 1)
insert into tblUsers (userName, personId, userActive, userIsNewPassword, userPassword) values ('SALEM',1, 1, 0, 'Ab123456')