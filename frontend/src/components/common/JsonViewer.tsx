function formatKey(key: string): string {
  return key
    .replace(/([A-Z])/g, ' $1')
    .replace(/^./, (s) => s.toUpperCase())
    .trim();
}

function isSimple(v: unknown): boolean {
  return v === null || typeof v === 'string' || typeof v === 'number' || typeof v === 'boolean';
}

function JsonViewerObject({ data }: { data: Record<string, unknown> }) {
  const entries = Object.entries(data).filter(([, v]) => v !== null && v !== undefined);

  return (
    <table className="w-full text-xs">
      <tbody>
        {entries.map(([key, value]) => (
          <tr key={key}>
            <td className="text-[#6C757D] pr-3 py-0.5 align-top whitespace-nowrap w-[1%]">{formatKey(key)}</td>
            <td className="py-0.5">{formatValue(value)}</td>
          </tr>
        ))}
      </tbody>
    </table>
  );
}

function formatValue(v: unknown): React.ReactNode {
  if (v === null || v === undefined) return null;
  if (isSimple(v)) return <span className="text-gray-800">{String(v)}</span>;

  if (Array.isArray(v)) {
    if (v.length === 0) return <span className="text-gray-500 italic">Empty array</span>;
    return (
      <div className="space-y-2">
        {v.map((item, i) => (
          <div key={i} className="border border-[#E9ECEF] rounded p-2">
            <div className="text-[#6C757D] text-[10px] font-semibold mb-1">Item {i + 1}</div>
            {isSimple(item) ? (
              <span className="text-gray-800">{String(item)}</span>
            ) : (
              <JsonViewerObject data={item as Record<string, unknown>} />
            )}
          </div>
        ))}
      </div>
    );
  }

  if (typeof v === 'object') {
    const obj = v as Record<string, unknown>;
    if (Object.keys(obj).length === 0) return <span className="text-gray-500 italic">Empty object</span>;
    return (
      <div className="border border-[#E9ECEF] rounded p-2">
        <JsonViewerObject data={obj} />
      </div>
    );
  }

  return <span className="text-gray-800">{String(v)}</span>;
}

interface JsonViewerProps {
  data: string | null | undefined;
  label?: string;
}

export function JsonViewer({ data, label }: JsonViewerProps) {
  if (!data) return null;

  let parsed: unknown;
  try {
    parsed = JSON.parse(data);
  } catch {
    return <span className="text-red-500 text-xs">Invalid JSON data</span>;
  }

  return (
    <div>
      {label && <span className="block text-xs font-semibold text-[#6C757D] uppercase tracking-wider mb-2">{label}</span>}
      {Array.isArray(parsed) ? (
        parsed.length === 0 ? (
          <span className="text-gray-500 text-xs italic">Empty array</span>
        ) : (
          <div className="space-y-2">
            {parsed.map((item, i) => (
              <div key={i} className="border border-[#E9ECEF] rounded p-2">
                <div className="text-[#6C757D] text-[10px] font-semibold mb-1">Item {i + 1}</div>
                {isSimple(item) ? (
                  <span className="text-gray-800 text-xs">{String(item)}</span>
                ) : (
                  <JsonViewerObject data={item as Record<string, unknown>} />
                )}
              </div>
            ))}
          </div>
        )
      ) : typeof parsed === 'object' && parsed !== null ? (
        Object.keys(parsed as Record<string, unknown>).length === 0 ? (
          <span className="text-gray-500 text-xs italic">Empty object</span>
        ) : (
          <JsonViewerObject data={parsed as Record<string, unknown>} />
        )
      ) : (
        <span className="text-gray-800 text-xs">{String(parsed)}</span>
      )}
    </div>
  );
}
