# TODO - SV22T1020337.Shop (Customer Features)

## 1) Infrastructure setup
- [x] Configure Program.cs: Authentication (Cookie), Session, HttpContextAccessor
- [x] Configure route default to Catalog/Index
- [x] Initialize BusinessLayers.Configuration with LiteCommerceDB
- [x] Configure vi-VN culture

## 2) AppCodes & cart/session/auth helpers
- [x] Create ApplicationContext
- [x] Create CustomerAuthHelper
- [x] Create ShoppingCartService
- [x] Create CartItemModel and fix compile references

## 3) ViewModels for customer flow
- [x] LoginModel
- [x] RegisterModel
- [x] ChangePasswordModel
- [x] ProfileModel
- [x] ProductFilterInputModel
- [x] CheckoutModel

## 4) Controllers
- [x] Create AccountController (Login/Register/Logout/ChangePassword/AccessDenied)
- [x] Create CatalogController
- [x] Create CartController
- [x] Create OrderController
- [x] Create ProfileController

## 5) Views (non-AdminLTE)
- [x] Account views
- [x] Catalog views
- [x] Cart views
- [x] Order views
- [x] Profile views

## 6) Error handling
- [x] Add try-catch for all actions in Shop controllers
- [x] Log exceptions and return safe messages

## 7) Build & test
- [x] Build Shop project
- [x] Critical path test
- [ ] Thorough test
