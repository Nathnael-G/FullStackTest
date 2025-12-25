interface User {
  id: number;
  name: string;
}
import { useEffect, useState } from "react";

function App() {
  const [users, setUsers] = useState<User[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    fetch("http://localhost:5016/users") // your .NET API endpoint
      .then((res) => res.json())
      .then((data: User[]) => {
        setUsers(data);
        setLoading(false);
      })
      .catch((err) => {
        console.error("Error fetching users:", err);
        setLoading(false);
      });
  }, []);

  if (loading) return <div>Loading...</div>;

  return (
    <div className="flex justify-center items-center h-dvh">
      <div>
      <h1 className="text-2xl font-bold text-red-500">Users</h1>
      <ul>
        {users.map((u) => (
          <li key={u.id}>{u.name}</li>
        ))}
      </ul>
      </div>
    </div>
  );
}

export default App;
