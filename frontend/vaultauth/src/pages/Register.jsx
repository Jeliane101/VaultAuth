import { useState } from "react";
import { useNavigate } from "react-router-dom";
import "../styles/register.css";
import logo from "../assets/vaultauth-logo1.png";
import { FaEye, FaEyeSlash } from "react-icons/fa";


export default function RegisterPage() {
  const [formData, setFormData] = useState({
    firstName: "",
    lastName: "",
    email: "",
    password: "",
    profileImage: null
  });
  const [message, setMessage] = useState("");
  const [isError, setIsError] = useState(false);
  const navigate = useNavigate();
const [showPassword, setShowPassword] = useState(false);
const [showHint, setShowHint] = useState(false);


  const handleChange = (e) => {
    const { name, value } = e.target;
    setFormData({ ...formData, [name]: value });
  };

  const handleFileChange = (e) => {
    setFormData({ ...formData, profileImage: e.target.files[0] });
  };

  const handleSubmit = async (e) => {
    e.preventDefault();

    const data = new FormData();
    data.append("FirstName", formData.firstName);
    data.append("LastName", formData.lastName);
    data.append("Email", formData.email);
    data.append("Password", formData.password);
    if (formData.profileImage) {
      data.append("ProfileImage", formData.profileImage);
    }

    try {
      const response = await fetch("http://localhost:5000/api/auth/register", {
        method: "POST",
        body: data
      });

      if (response.ok) {
        setIsError(false);
        setMessage("Registration successful! Redirecting to login...");
        setTimeout(() => navigate("/login"), 2000);
      } else {
        setIsError(true);
        setMessage("Registration failed. Please try again.");
      }
    } catch (error) {
      setIsError(true);
      setMessage("Something went wrong. Please try again later.");
    }
  };

  return (
    <div className="register-container">
      <img src={logo} alt="VaultAuth Logo" className="logo" />
      <h2>Sign Up</h2>
      <form onSubmit={handleSubmit}>
        <label>
          First Name <span className="required">*</span>
          <input
            type="text"
            name="firstName"
            value={formData.firstName}
            onChange={handleChange}
            required
          />
        </label>

        <label>
          Last Name <span className="required">*</span>
          <input
            type="text"
            name="lastName"
            value={formData.lastName}
            onChange={handleChange}
            required
          />
        </label>

        <label>
          Email <span className="required">*</span>
          <input
            type="email"
            name="email"
            value={formData.email}
            onChange={handleChange}
            required
          />
        </label>

      <label>
  Password <span className="required">*</span>
  <div className="password-wrapper">
    <input
      type={showPassword ? "text" : "password"}
      name="password"
      placeholder="Password"
      value={formData.password}
      onChange={handleChange}
      onFocus={() => setShowHint(true)}
      onBlur={() => setShowHint(false)}
      required
    />
    <span
  className="toggle-password"
  onClick={() => setShowPassword(!showPassword)}
>
  {showPassword ? <FaEyeSlash /> : <FaEye />}
</span>
  </div>
  {showHint && (
    <p className="password-hint">
      Password should be at least 8 characters long and must contain at least 1 Uppercase, 1 lowercase, and 1 special character.
    </p>
  )}
</label>


        <label>
          Profile Picture
          <input
            type="file"
            name="profileImage"
            accept=".jpg,.jpeg,.png"
            onChange={handleFileChange}
          />
        </label>

        <button type="submit">Register</button>
      </form>

      {message && (
        <div className={isError ? "error-message" : "success-message"}>
          {message}
        </div>
      )}
    </div>
  );
}
