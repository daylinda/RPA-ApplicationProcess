/// <reference types="chrome" />

import { CONFIG } from "../config";


type CapturedApplication = {
  id: string;
  url: string;
  sourceDomain: string;
  jobTitle?: string;
  jobNumber?: string;
  applicationId?: string;
  appliedAt: string; // ISO
  status: "Applied";
};

const KEY = "applications";
const LAST_KEY = "lastCapturedApplication";

async function saveToGoogleSheet(app: any) {
  const WEB_APP_URL = CONFIG.GOOGLE_SHEETS.WEB_APP_URL;

    await fetch(WEB_APP_URL, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(app),
  });
}

function storageGet<T>(key: string): Promise<T | undefined> {
  return new Promise((resolve) => {
    chrome.storage.local.get([key], (res) => resolve(res[key] as T | undefined));
  });
}

function storageSet(obj: Record<string, unknown>): Promise<void> {
  return new Promise((resolve, reject) => {
    chrome.storage.local.set(obj, () => {
      const err = chrome.runtime.lastError;
      if (err) reject(err);
      else resolve();
    });
  });
}

async function addApplication(app: CapturedApplication) {
  const existing = (await storageGet<CapturedApplication[]>(KEY)) ?? [];
  const next = [app, ...existing];
  await storageSet({ [KEY]: next, [LAST_KEY]: app });
  await saveToGoogleSheet(app);

  // Badge shows “1” meaning “new capture”
  chrome.action.setBadgeText({ text: "1" });
  chrome.action.setBadgeBackgroundColor({ color: "#1976d2" });

  // Optional notification
  chrome.notifications.create({
    type: "basic",
    iconUrl: "icon128.png", // add icons later, or remove notifications
    title: "Application captured",
    message: `${app.jobTitle ?? "Job"}${app.applicationId ? ` • ID: ${app.applicationId}` : ""}`
  });
}

chrome.runtime.onInstalled.addListener(() => {
  chrome.action.setBadgeText({ text: "" });
});

chrome.runtime.onMessage.addListener((msg, _sender, sendResponse) => {
  (async () => {
    if (msg?.type === "APPLIED_DETECTED") {
      await addApplication(msg.payload as CapturedApplication);
      sendResponse({ ok: true });
      return;
    }

    if (msg?.type === "CLEAR_BADGE") {
      chrome.action.setBadgeText({ text: "" });
      sendResponse({ ok: true });
      return;
    }

    sendResponse({ ok: false });
  })();

  // keep the message channel open for async
  return true;
});