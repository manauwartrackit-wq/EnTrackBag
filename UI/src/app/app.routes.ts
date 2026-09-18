import { Routes } from "@angular/router";
import { LoginComponent } from "./login/login.component";
import { SummaryComponent } from "./dashboard/summary/summary.component";
import { SlaComponent } from "./dashboard/sla/sla.component";
import { DeviceStatusComponent } from "./device-status/device-status.component";
import { authGuard } from "./core/auth.guard";
import { TagReportComponent } from "./tag-report/tag-report.component";
import { BagJourneyComponent } from "./bag-journey/bag-journey.component";
import { AdministrationComponent } from "./administration/administration.component";
import { BagJourneyConfigurationComponent } from "./bag-journey-configuration/bag-journey-configuration.component";

export const routes: Routes = [
  { path: "login", component: LoginComponent },
  {
    path: "dashboard/summary",
    component: SummaryComponent,
    canActivate: [authGuard],
    data: {
      permission: "Dashboard",
      accessType: "VIEW",
      title: "Dashboard",
      subtitle: "Operational summary and current bag flow.",
    },
  },
  {
    path: "dashboard/sla",
    component: SlaComponent,
    canActivate: [authGuard],
    data: {
      permission: "Dashboard.SLA",
      accessType: "VIEW",
      title: "Dashboard",
      subtitle: "Service-level agreement monitoring.",
    },
  },
  {
    path: "device-status",
    component: DeviceStatusComponent,
    canActivate: [authGuard],
    data: {
      permission: "DeviceStatus",
      accessType: "VIEW",
      title: "Device & System Status",
      subtitle: "Current communication and device status from verified operational data.",
    },
  },
  {
    path: "tag-report",
    component: TagReportComponent,
    canActivate: [authGuard],
    data: {
      permission: "TagReport",
      accessType: "VIEW",
      title: "Reports",
      subtitle: "Tag and bag reporting.",
    },
  },
  {
    path: "bag-journey",
    component: BagJourneyComponent,
    canActivate: [authGuard],
    data: {
      permission: "BagJourney",
      accessType: "VIEW",
      title: "Tag / Bag Tracking",
      subtitle: "Track bag movement through the journey stages.",
    },
  },
  {
    path: "administration",
    component: AdministrationComponent,
    canActivate: [authGuard],
    data: {
      permission: "Administration",
      accessType: "VIEW",
      title: "Administration",
      subtitle: "Manage users, roles, active sessions and system audit activity.",
    },
  },
  {
    path: "bag-journey-configuration",
    component: BagJourneyConfigurationComponent,
    canActivate: [authGuard],
    data: {
      permission: "BagJourney.Configuration",
      accessType: "VIEW",
      title: "Bag Journey Configuration",
      subtitle: "Configure bag journey stages, routing and service-level thresholds",
    },
  },
  { path: "", pathMatch: "full", redirectTo: "login" },
  { path: "**", redirectTo: "login" },
];
