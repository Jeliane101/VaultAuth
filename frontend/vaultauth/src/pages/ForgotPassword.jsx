import { useState } from "react";
import logo from "../assets/vaultauth-logo1.png";
import defaultAvatar from "../assets/default.png";
import "../styles/dashboard.css";

export default function Dashboard({ user }) {
  const [showMenu, setShowMenu] = useState(false);

  return (
    <div className="dashboard-container">
      {/* Header */}
      <header className="dashboard-header">
        <div className="header-left">
          <img src={logo} alt="VaultAuth Logo" className="logo" />
          <span className="app-name">VaultAuth</span>
        </div>
        <div className="header-right">
          <span className="user-name" onClick={() => setShowMenu(!showMenu)}>
            {user.name}
          </span>
          {showMenu && (
            <div className="dropdown-menu">
              <button onClick={() => console.log("Logout clicked")}>
                Logout
              </button>
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
            <h2>{user.firstName}</h2>
            <h2>{user.lastName}</h2>
            <p>{user.email}</p>
            <button className="edit-profile-btn">Edit Profile</button>
          </div>
        </div>
      </main>
    </div>
  );
}
