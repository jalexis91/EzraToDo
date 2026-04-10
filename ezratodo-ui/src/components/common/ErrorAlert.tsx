import React from 'react';

interface ErrorAlertProps {
  title: string;
  errors?: Record<string, string[]>;
  onDismiss: () => void;
}

const ErrorAlert: React.FC<ErrorAlertProps> = ({ title, errors, onDismiss }) => {
  return (
    <div style={{
      marginBottom: '1rem',
      padding: '1rem',
      backgroundColor: '#fef2f2',
      border: '1px solid #fecaca',
      borderRadius: '0.375rem',
      color: '#991b1b',
      position: 'relative'
    }}>
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start' }}>
        <div>
          <h3 style={{ fontWeight: 600, margin: 0 }}>{title}</h3>
          {errors && Object.keys(errors).length > 0 && (
            <ul style={{ marginTop: '0.5rem', paddingLeft: '1.25rem', listStyleType: 'disc', fontSize: '0.875rem' }}>
              {Object.entries(errors).map(([field, messages]) => (
                <li key={field}>
                  <strong>{field}:</strong> {messages.join(', ')}
                </li>
              ))}
            </ul>
          )}
        </div>
        <button
          onClick={onDismiss}
          style={{
            background: 'none',
            border: 'none',
            color: '#dc2626',
            fontWeight: 600,
            cursor: 'pointer',
            fontSize: '1.25rem',
            lineHeight: 1
          }}
          aria-label="Dismiss error"
        >
          ×
        </button>
      </div>
    </div>
  );
};

export default ErrorAlert;
