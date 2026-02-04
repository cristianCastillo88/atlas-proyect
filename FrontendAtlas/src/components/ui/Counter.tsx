import { useStore } from '@nanostores/react';
import { counter, increment } from '../../stores/counter';

export default function Counter() {
  const count = useStore(counter);

  return (
    <div className="flex flex-col items-center">
      <p className="text-2xl mb-4">Count: {count}</p>
      <button
        className="px-4 py-2 bg-blue-500 text-white rounded hover:bg-blue-600"
        onClick={increment}
      >
        Increment
      </button>
    </div>
  );
}