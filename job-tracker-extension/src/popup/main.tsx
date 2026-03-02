/// <reference types="chrome" />
import { useEffect, useState } from "react";
import ReactDOM from "react-dom/client";

type CapturedApplication = {
  id: string;
  url: string;
  sourceDomain: string;
  jobTitle?: string;
  jobNumber?: string;
  applicationId?: string;
  appliedAt: string;
  status: "Applied";
};

const LAST_KEY = "lastCapturedApplication";

function App() {
  const [last, setLast] = useState<CapturedApplication | null>(null);

  useEffect(() => {
    chrome.storage.local.get([LAST_KEY], (res) => {
      setLast((res[LAST_KEY] as CapturedApplication) ?? null);
    });

    // Clear badge when popup opens
    chrome.runtime.sendMessage({ type: "CLEAR_BADGE" });
  }, []);

  return (
    <div style={{ width: 340, padding: 12, fontFamily: "system-ui" }}>
      <h3 style={{ margin: "0 0 10px" }}>Job Tracker</h3>

      {!last ? (
        <div style={{ fontSize: 13, opacity: 0.8 }}>
          No captured “Applied” event yet. Submit an application and land on the confirmation page.
        </div>
      ) : (
        <div style={{ fontSize: 13, lineHeight: 1.4 }}>
          <div><b>Title:</b> {last.jobTitle ?? "—"}</div>
          <div><b>Job #:</b> {last.jobNumber ?? "—"}</div>
          <div><b>Application ID:</b> {last.applicationId ?? "—"}</div>
          <div><b>Site:</b> {last.sourceDomain}</div>
          <div style={{ marginTop: 8 }}>
            <a href={last.url} target="_blank" rel="noreferrer">Open page</a>
          </div>
        </div>
      )}
    </div>
  );
}

ReactDOM.createRoot(document.getElementById("root")!).render(<App />);