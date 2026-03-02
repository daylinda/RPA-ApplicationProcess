/// <reference types="chrome" />

function domainFromUrl(url: string) {
  try {
    return new URL(url).hostname;
  } catch {
    return "";
  }
}

function looksLikeSubmissionPage(allText: string) {
  const t = allText.toLowerCase();

  const phrases = [
    "thank you for applying",
    "application submitted",
    "application received",
    "successfully applied",
    "submission confirmed",
    "your application has been sent",
    "application has been sent",
    "your application was sent",
    "we've received your application",
    "we have received your application",
    "application complete",
    "application successfully",
    "nice work" // <-- add this for your screenshot
  ];

  return phrases.some((p) => t.includes(p));
}

function collectPageText(): string {
  const parts: string[] = [];

  // 1) Normal visible text
  parts.push(document.body?.innerText ?? "");

  // 2) Useful attributes + headings/buttons (often contain success copy)
  const attrSelectors = [
    "[aria-label]",
    "[aria-labelledby]",
    "[title]",
    "img[alt]",
    "button",
    "a",
    "h1,h2,h3",
    "[role='alert']",
    "[role='status']"
  ].join(",");

  document.querySelectorAll(attrSelectors).forEach((el) => {
    const aria = el.getAttribute("aria-label") || "";
    const title = el.getAttribute("title") || "";
    const alt = (el as HTMLImageElement).alt || "";
    const txt = (el as HTMLElement).innerText || el.textContent || "";

    if (aria) parts.push(aria);
    if (title) parts.push(title);
    if (alt) parts.push(alt);
    if (txt) parts.push(txt);
  });

  return parts.join("\n");
}

function extractFields(allText: string) {
  const cleaned = allText.replace(/\s+/g, " ");

  const applicationId =
    cleaned.match(/application\s*(id|number)\s*[:#-]?\s*([A-Z0-9-]{5,})/i)?.[2] ??
    cleaned.match(/submission\s*(id|number)\s*[:#-]?\s*([A-Z0-9-]{5,})/i)?.[2];

  const jobNumber =
    cleaned.match(/job\s*(id|number|no\.?)\s*[:#-]?\s*([A-Z0-9-]{4,})/i)?.[2] ??
    cleaned.match(/requisition\s*(id|number)\s*[:#-]?\s*([A-Z0-9-]{4,})/i)?.[2] ??
    cleaned.match(/\breq(?:uisition)?\s*[:#-]?\s*([A-Z0-9-]{4,})\b/i)?.[1];

  const company =
    cleaned.match(/application has been sent to\s+(.+?)(?:\.|$)/i)?.[1]?.trim() ??
    cleaned.match(/application (?:was )?sent to\s+(.+?)(?:\.|$)/i)?.[1]?.trim();

  const jobTitle =
    document.querySelector("h1")?.textContent?.trim() ||
    document.title?.trim() ||
    undefined;

  return { applicationId, jobNumber, company, jobTitle };
}

// Prevent double-saving on SPA re-renders
let alreadySentForUrl = "";

function attemptCapture(): boolean {
  const allText = collectPageText();

  if (!looksLikeSubmissionPage(allText)) return false;

  // Avoid duplicate saves on same URL
  if (alreadySentForUrl === location.href) return true;

  const { applicationId, jobNumber, company, jobTitle } = extractFields(allText);

  const payload = {
    id: crypto.randomUUID(),
    url: location.href,
    sourceDomain: domainFromUrl(location.href),
    company,
    jobTitle,
    jobNumber,
    applicationId,
    appliedAt: new Date().toISOString(),
    status: "Applied" as const
  };

  chrome.runtime.sendMessage({ type: "APPLIED_DETECTED", payload });
  alreadySentForUrl = location.href;
  return true;
}

// Try now, then retry a few times (for delayed rendering)
let tries = 0;
const maxTries = 10;
const interval = setInterval(() => {
  tries++;
  const done = attemptCapture();
  if (done || tries >= maxTries) clearInterval(interval);
}, 750);

// Observe changes for SPAs
const obs = new MutationObserver(() => {
  if (attemptCapture()) obs.disconnect();
});
obs.observe(document.documentElement, {
  subtree: true,
  childList: true,
  characterData: true
});