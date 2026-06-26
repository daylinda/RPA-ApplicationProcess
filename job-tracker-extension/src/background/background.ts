/// <reference types="chrome" />

import { CONFIG } from "../config";

type JobEntry = {
  id: string;
  url: string;
  sourceDomain: string;
  jobTitle?: string;
  company?: string;
  location?: string;
  salary?: string;
  jobNumber?: string;
  applicationId?: string;
  appliedAt: string; // ISO
  status: "Applied" | "Saved";
};

const KEY = "applications";
const LAST_KEY = "lastCapturedApplication";

async function saveToGoogleSheet(app: JobEntry) {
  // Use text/plain to avoid CORS preflight — Google Apps Script doesn't
  // respond to OPTIONS requests, which silently blocks application/json POSTs.
  const res = await fetch(CONFIG.GOOGLE_SHEETS.WEB_APP_URL, {
    method: "POST",
    headers: { "Content-Type": "text/plain" },
    body: JSON.stringify(app),
  });
  if (!res.ok) {
    throw new Error(`Google Sheets POST failed: ${res.status} ${res.statusText}`);
  }
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

async function addEntry(entry: JobEntry) {
  const existing = (await storageGet<JobEntry[]>(KEY)) ?? [];
  await storageSet({ [KEY]: [entry, ...existing], [LAST_KEY]: entry });
  await saveToGoogleSheet(entry);
}

// Scraper executed inside the active tab via executeScript
function scrapeJobPage(): Record<string, string | undefined> {
  function domainFromUrl(url: string) {
    try { return new URL(url).hostname; } catch { return ""; }
  }

  const text = (document.body?.innerText ?? "").replace(/\s+/g, " ");

  // --- Job title ---
  const jobTitle =
    document.querySelector<HTMLElement>('[data-automation="job-detail-title"]')?.innerText?.trim() ||
    document.querySelector<HTMLElement>(".job-title, .jobTitle, [class*='jobTitle'], h1.title")?.innerText?.trim() ||
    document.querySelector("h1")?.innerText?.trim() ||
    document.title?.trim();

  // --- Company ---
  const company =
    document.querySelector<HTMLElement>('[data-automation="advertiser-name"], .company-name, [class*="companyName"], [class*="company-name"]')?.innerText?.trim() ||
    (document.querySelector<HTMLElement>('meta[property="og:site_name"]')?.getAttribute("content") ?? undefined) ||
    text.match(/(?:at|by)\s+([A-Z][A-Za-z0-9 &,.']+?)(?:\s*[-·|•]|\s+in\s|\s+–|\s*$)/m)?.[1]?.trim();

  // --- Location ---
  const jobLocation =
    document.querySelector<HTMLElement>('[data-automation="job-detail-location"], .location, [class*="location"], [class*="jobLocation"]')?.innerText?.trim() ||
    (document.querySelector<HTMLElement>('meta[name="geo.placename"]')?.getAttribute("content") ?? undefined) ||
    text.match(/(?:location|based in|remote[- ]?ok)[\s:]+([A-Za-z ,]+?)(?:\n|·|•|-|$)/im)?.[1]?.trim();

  // --- Salary ---
  const salary =
    document.querySelector<HTMLElement>('[data-automation="job-detail-salary"], [class*="salary"], [class*="compensation"]')?.innerText?.trim() ||
    text.match(/\$[\d,]+(?:\s*[-–]\s*\$[\d,]+)?(?:\s*(?:\/yr|\/year|\/hour|\/hr|k))?/i)?.[0];

  return {
    jobTitle,
    company,
    location: jobLocation,
    salary,
    url: window.location.href,
    sourceDomain: domainFromUrl(window.location.href),
  };
}

async function saveCurrentJob(): Promise<void> {
  const [tab] = await chrome.tabs.query({ active: true, currentWindow: true });
  if (!tab?.id || !tab.url) throw new Error("No active tab");

  const results = await chrome.scripting.executeScript({
    target: { tabId: tab.id },
    func: scrapeJobPage,
  });

  const scraped = results[0]?.result ?? {};

  const entry: JobEntry = {
    id: crypto.randomUUID(),
    url: tab.url,
    sourceDomain: new URL(tab.url).hostname,
    jobTitle: scraped["jobTitle"] || tab.title || undefined,
    company: scraped["company"],
    location: scraped["location"],
    salary: scraped["salary"],
    appliedAt: new Date().toISOString(),
    status: "Saved",
  };

  await addEntry(entry);

  chrome.notifications.create({
    type: "basic",
    iconUrl: "icon128.png",
    title: "Job saved",
    message: `${entry.jobTitle ?? "Job"}${entry.company ? ` @ ${entry.company}` : ""}`,
  });
}

chrome.runtime.onInstalled.addListener(() => {
  chrome.action.setBadgeText({ text: "" });
});

chrome.runtime.onMessage.addListener((msg, _sender, sendResponse) => {
  (async () => {
    if (msg?.type === "APPLIED_DETECTED") {
      await addEntry(msg.payload as JobEntry);
      chrome.action.setBadgeText({ text: "1" });
      chrome.action.setBadgeBackgroundColor({ color: "#1976d2" });
      chrome.notifications.create({
        type: "basic",
        iconUrl: "icon128.png",
        title: "Application captured",
        message: `${(msg.payload as JobEntry).jobTitle ?? "Job"}${(msg.payload as JobEntry).applicationId ? ` • ID: ${(msg.payload as JobEntry).applicationId}` : ""}`,
      });
      sendResponse({ ok: true });
      return;
    }

    if (msg?.type === "SAVE_JOB") {
      try {
        await saveCurrentJob();
        sendResponse({ ok: true });
      } catch (e) {
        console.error("SAVE_JOB failed:", e);
        sendResponse({ ok: false, error: String(e) });
      }
      return;
    }

    if (msg?.type === "CLEAR_BADGE") {
      chrome.action.setBadgeText({ text: "" });
      sendResponse({ ok: true });
      return;
    }

    sendResponse({ ok: false });
  })();

  return true; // keep channel open for async
});
