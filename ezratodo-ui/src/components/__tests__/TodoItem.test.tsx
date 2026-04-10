import { render, screen, fireEvent, act } from '@testing-library/react';
import TodoItem from '../TodoItem';
import { Todo } from '../../types/todo';

describe('TodoItem', () => {
  const mockTodo: Todo = {
    id: 1,
    title: 'Test todo',
    isCompleted: false,
    createdAt: '2026-03-25T00:00:00Z',
    updatedAt: '2026-03-25T00:00:00Z',
  };

  it('renders todo title', () => {
    render(
      <TodoItem
        todo={mockTodo}
        onToggleComplete={jest.fn()}
        onDelete={jest.fn()}
      />
    );

    expect(screen.getByText('Test todo')).toBeInTheDocument();
  });

  it('calls onToggleComplete when checkbox is clicked', async () => {
    const onToggleComplete = jest.fn().mockResolvedValue(undefined);

    render(
      <TodoItem
        todo={mockTodo}
        onToggleComplete={onToggleComplete}
        onDelete={jest.fn()}
      />
    );

    const checkbox = screen.getByRole('checkbox');
    await act(async () => {
      fireEvent.click(checkbox);
    });

    expect(onToggleComplete).toHaveBeenCalledWith(1, false);
  });

  it('calls onDelete when delete button is clicked and confirmed', async () => {
    const onDelete = jest.fn().mockResolvedValue(undefined);
    window.confirm = jest.fn(() => true);

    render(
      <TodoItem
        todo={mockTodo}
        onToggleComplete={jest.fn()}
        onDelete={onDelete}
      />
    );

    const deleteButton = screen.getByRole('button', { name: /delete/i });
    await act(async () => {
      fireEvent.click(deleteButton);
    });

    expect(window.confirm).toHaveBeenCalled();
    expect(onDelete).toHaveBeenCalledWith(1);
  });
});
