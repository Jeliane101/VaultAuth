import { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import logo from "../assets/vaultauth-logo1.png";
import defaultAvatar from "../assets/default.png";
import "../styles/dashboard.css";

export default function Dashboard() {
  const navigate = useNavigate();

const handleEditProfile = () => {
  navigate("/update-profile"); 
};

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
  console.log("Profile data:", data); 

  setUser({
    id: data.id,
    firstName: data.firstName,   
    lastName: data.lastName,
    email: data.email,
     profileImage: data.imageURL 
    ? `http://localhost:5000${data.imageURL}` 
    : null,
    name: `${data.firstName} ${data.lastName}` 
  });
} 
else if (response.status === 401) {
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

const handleLogout = async () => {
  try {
    const token = localStorage.getItem("accessToken");

    const response = await fetch("http://localhost:5000/api/auth/logout", {
      method: "POST", 
      headers: {
        "Authorization": `Bearer ${token}`,
        "Content-Type": "application/json"
      }
    });

    if (response.ok) {
      localStorage.removeItem("accessToken");
      window.location.href = "/login";
    } else {
      console.error("Logout failed:", await response.json());
    }
  } catch (err) {
    console.error("Error logging out:", err);
  }
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
            <p>First Name: {user.firstName}</p>
            <p>Last Name: {user.lastName}</p>
            <p>Email: {user.email}</p>
            <button 
  className="edit-profile-btn" 
  onClick={handleEditProfile}
>
  Edit Profile
</button>

          </div>
        </div>
      </main>
    </div>
  );
}
