export function printHtml(title: string, html: string) {
  const win = window.open('', '_blank');
  if (!win) return;
  win.document.write(`<!DOCTYPE html><html><head><title>${title}</title>
    <style>
      body { font-family: Arial, Helvetica, sans-serif; padding: 30px; color: #212529; }
      h1 { font-size: 20px; margin-bottom: 4px; }
      .sub { color: #6C757D; font-size: 13px; margin-bottom: 20px; }
      table { width: 100%; border-collapse: collapse; margin: 16px 0; font-size: 13px; }
      th, td { text-align: left; padding: 8px 10px; border-bottom: 1px solid #E9ECEF; }
      th { background: #F8F9FA; font-weight: 600; }
      .label { color: #6C757D; font-size: 11px; text-transform: uppercase; letter-spacing: 0.5px; margin-bottom: 4px; }
      .value { margin-bottom: 16px; font-size: 14px; }
      .grid { display: grid; grid-template-columns: 1fr 1fr; gap: 12px; margin: 16px 0; }
      hr { border: none; border-top: 1px solid #E9ECEF; margin: 20px 0; }
      .footer { text-align: center; font-size: 11px; color: #6C757D; margin-top: 30px; border-top: 1px solid #E9ECEF; padding-top: 12px; }
      .badge { display: inline-block; padding: 2px 10px; border-radius: 12px; font-size: 12px; font-weight: 500; }
      @media print { body { padding: 15px; } }
    </style></head><body>
    ${html}
    <div class="footer">HealthSync - Generated on ${new Date().toLocaleString('en-PH')}</div>
    </body></html>`);
  win.document.close();
  win.focus();
  setTimeout(() => win.print(), 300);
}
