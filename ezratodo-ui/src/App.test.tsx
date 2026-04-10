import React from 'react';
import { render, screen } from '@testing-library/react';
import App from './App';

test('renders My Todos header', () => {
  render(<App />);
  const headerElement = screen.getByText(/My Todos/i);
  expect(headerElement).toBeInTheDocument();
});
