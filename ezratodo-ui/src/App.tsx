import React from 'react';
import TodoList from './components/TodoList';

function App() {
  return (
    <div className="App" style={{
      minHeight: '100vh',
      backgroundColor: '#f3f4f6',
      padding: '2rem 0'
    }}>
      <TodoList />
    </div>
  );
}

export default App;
