import { useEffect, useState } from "react";
import logo from "../assets/vaultauth-logo1.png";
import defaultAvatar from "../assets/default.png";
import "../styles/dashboard.css";

export default function Dashboard() {
  const [user, setUser] = useState(null);
  const [showMenu, setShowMenu] = useState(false);

  useEffect(() => {
    const fetchProfile = async () => {
      try {
        const token = localStorage.getItem("accessToken"); 
        const response = await fetch("http://localhost:5000/api/auth/profile", {
          method: "GET",
          headers: {
            "Authorization": `Bearer ${token}`,
            "Content-Type": "application/json"
          }
        });

        if (response.ok) {
          const data = await response.json();
          setUser({
            id: data.ID,
            name: `${data.FirstName} ${data.LastName}`, // ✅ PascalCase keys
            email: data.Email,
            profileImage: data.ImageURL
          });
        } else if (response.status === 401) {
          // Token invalid or expired → redirect to login
          localStorage.removeItem("accessToken");
          window.location.href = "/login";
        } else {
          console.error("Failed to fetch profile");
        }
      } catch (err) {
        console.error("Error fetching profile:", err);
      }
    };

    fetchProfile();
  }, []);

  const handleLogout = () => {
    localStorage.removeItem("accessToken");
    window.location.href = "/login";
  };

  if (!user) {
    return <div>Loading profile...</div>;
  }

  return (
    <div className="dashboard-container">
      {/* Header */}
      <header className="dashboard-header">
        <div className="header-left">
          <img src={logo} alt="VaultAuth Logo" className="logo" />
          <span className="app-name">VaultAuth</span>
        </div>
        <div className="header-right">
          <span
            className="user-name"
            onClick={() => setShowMenu(!showMenu)}
          >
            {user.name}
          </span>
          {showMenu && (
            <div className="dropdown-menu">
              <button onClick={handleLogout}>Logout</button>
            </div>
          )}
        </div>
      </header>

      {/* Main Content */}
      <main className="dashboard-main">
        <div className="profile-section">
          <img
            src={user.profileImage || defaultAvatar}
            alt="Profile"
            className="profile-image"
          />
          <div className="profile-info">
            <h2>{user.name}</h2>
            <p>{user.email}</p>
            <button className="edit-profile-btn">Edit Profile</button>
          </div>
        </div>
      </main>
    </div>
  );
}
