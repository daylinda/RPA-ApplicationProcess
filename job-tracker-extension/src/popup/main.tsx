/// <reference types="chrome" />
import { useEffect, useState } from "react";
import ReactDOM from "react-dom/client";

type SavedJob = {
  id: string;
  url: string;
  sourceDomain: string;
  jobTitle?: string;
  company?: string;
  location?: string;
  salary?: string;
  appliedAt: string;
  status: "Applied" | "Saved";
};

const LAST_KEY = "lastCapturedApplication";

type SaveState = "idle" | "saving" | "saved" | "error";

function App() {
  const [last, setLast] = useState<SavedJob | null>(null);
  const [saveState, setSaveState] = useState<SaveState>("idle");

  useEffect(() => {
    chrome.storage.local.get([LAST_KEY], (res) => {
      setLast((res[LAST_KEY] as SavedJob) ?? null);
    });
    chrome.runtime.sendMessage({ type: "CLEAR_BADGE" });
  }, []);

  function handleSaveJob() {
    setSaveState("saving");
    chrome.runtime.sendMessage({ type: "SAVE_JOB" }, (response) => {
      if (response?.ok) {
        setSaveState("saved");
        setTimeout(() => setSaveState("idle"), 2500);
      } else {
        setSaveState("error");
        setTimeout(() => setSaveState("idle"), 3000);
      }
    });
  }

  const btnLabel =
    saveState === "saving" ? "Saving…" :
    saveState === "saved"  ? "✓ Saved!" :
    saveState === "error"  ? "Error — try again" :
    "Save this job";

  const btnColor =
    saveState === "saved"  ? "#2e7d32" :
    saveState === "error"  ? "#c62828" :
    "#1976d2";

  return (
    <div style={{ width: 340, padding: 12, fontFamily: "system-ui" }}>
      <h3 style={{ margin: "0 0 12px" }}>Job Tracker</h3>

      {/* Save current job */}
      <div style={{ marginBottom: 16 }}>
        <button
          onClick={handleSaveJob}
          disabled={saveState === "saving"}
          style={{
            width: "100%",
            padding: "8px 12px",
            background: btnColor,
            color: "#fff",
            border: "none",
            borderRadius: 6,
            fontSize: 13,
            fontWeight: 600,
            cursor: saveState === "saving" ? "default" : "pointer",
          }}
        >
          {btnLabel}
        </button>
        <div style={{ fontSize: 11, color: "#666", marginTop: 4 }}>
          Saves the current page's job details to your Google Sheet
        </div>
      </div>

      {/* Divider */}
      <hr style={{ border: "none", borderTop: "1px solid #e0e0e0", margin: "0 0 12px" }} />

      {/* Last auto-captured application */}
      <div style={{ fontSize: 12, color: "#888", marginBottom: 6, fontWeight: 600, textTransform: "uppercase", letterSpacing: "0.05em" }}>
        Last auto-captured
      </div>
      {!last ? (
        <div style={{ fontSize: 13, opacity: 0.7 }}>
          No captured application yet. Submit an application and land on the confirmation page.
        </div>
      ) : (
        <div style={{ fontSize: 13, lineHeight: 1.6 }}>
          <div><b>Title:</b> {last.jobTitle ?? "—"}</div>
          {last.company && <div><b>Company:</b> {last.company}</div>}
          {last.location && <div><b>Location:</b> {last.location}</div>}
          <div><b>Site:</b> {last.sourceDomain}</div>
          <div style={{ marginTop: 6 }}>
            <a href={last.url} target="_blank" rel="noreferrer">Open page</a>
          </div>
        </div>
      )}
    </div>
  );
}

ReactDOM.createRoot(document.getElementById("root")!).render(<App />);
