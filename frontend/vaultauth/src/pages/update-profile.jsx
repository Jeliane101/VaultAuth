import { useState } from "react";
import { useNavigate } from "react-router-dom";
import logo from "../assets/vaultauth-logo1.png";
import "../styles/update-profile.css"; 

export default function UpdateProfile() {
  const [firstName, setFirstName] = useState("");
  const [lastName, setLastName] = useState("");
  const [email, setEmail] = useState("");
  const [profileImage, setProfileImage] = useState(null);
  const navigate = useNavigate();

  const handleSubmit = async (e) => {
    e.preventDefault();

    const token = localStorage.getItem("accessToken");
    const formData = new FormData();
    formData.append("FirstName", firstName);
    formData.append("LastName", lastName);
    formData.append("Email", email);
    if (profileImage) {
      formData.append("profileImage", profileImage);
    }

    try {
      const response = await fetch("http://localhost:5000/api/auth/update", {
        method: "PUT",
        headers: {
          "Authorization": `Bearer ${token}`
        },
        body: formData
      });

      if (response.ok) {
        alert("Profile updated successfully!");
        navigate("/dashboard");
      } else {
        const error = await response.json();
        alert("Update failed: " + error.message);
      }
    } catch (err) {
      console.error("Error updating profile:", err);
    }
  };

  return (
    <div className="update-profile-container">
      <div className="update-card">
      <img src={logo} alt="VaultAuth Logo" className="logo" />
        <h2>Update Profile</h2>
        <form onSubmit={handleSubmit}>
          <label>
            First Name:
            <input 
              type="text" 
              value={firstName} 
              onChange={(e) => setFirstName(e.target.value)} 
            />
          </label>
          <label>
            Last Name:
            <input 
              type="text" 
              value={lastName} 
              onChange={(e) => setLastName(e.target.value)} 
            />
          </label>
          <label>
            Email:
            <input 
              type="email" 
              value={email} 
              onChange={(e) => setEmail(e.target.value)} 
            />
          </label>
          <label>
            Profile Image:
            <input 
              type="file" 
              accept="image/*" 
              onChange={(e) => setProfileImage(e.target.files[0])} 
            />
          </label>
          <div className="button-group">
            <button type="submit" className="save-btn">Save Changes</button>
            <button 
              type="button" 
              className="back-btn" 
              onClick={() => navigate("/dashboard")}
            >
              Back
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}
