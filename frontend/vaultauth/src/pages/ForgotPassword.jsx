import { useState } from "react";
import { useNavigate } from "react-router-dom";
import "../styles/forgotP.css";
import logo from "../assets/vaultauth-logo1.png";
import { FaEye, FaEyeSlash } from "react-icons/fa";

export default function ForgotPasswordPage() {
  const [formData, setFormData] = useState({ email: "", newPassword: "" });
  const [showPassword, setShowPassword] = useState(false);
  const [message, setMessage] = useState("");
  const [isError, setIsError] = useState(false);
  const navigate = useNavigate();

  const handleChange = (e) => {
    const { name, value } = e.target;
    setFormData({ ...formData, [name]: value });
  };

  const handleSubmit = async (e) => {
    e.preventDefault();

    try {
      const response = await fetch("http://localhost:5000/api/auth/update-password", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(formData)
      });

      if (response.ok) {
        setIsError(false);
        setMessage("Password updated successfully! Redirecting to login...");
        setTimeout(() => navigate("/login"), 3000);
      } else {
        setIsError(true);
        setMessage("No account found with that email.");
      }
    } catch {
      setIsError(true);
      setMessage("Something went wrong. Please try again later.");
    }
  };

  return (
    <div className="forgot-container">
      <img src={logo} alt="VaultAuth Logo" className="logo" />
      <h2>Update Your Password</h2>
      <form onSubmit={handleSubmit}>
        <label>
          Email <span className="required">*</span>
          <input
            type="email"
            name="email"
            placeholder="Enter your email"
            value={formData.email}
            onChange={handleChange}
            required
          />
        </label>

        <label>
          New Password <span className="required">*</span>
          <div className="password-wrapper">
            <input
              type={showPassword ? "text" : "password"}
              name="newPassword"
              placeholder="Enter new password"
              value={formData.newPassword}
              onChange={handleChange}
              required
            />
            <span
              className="toggle-password"
              onClick={() => setShowPassword(!showPassword)}
              title={showPassword ? "Hide password" : "Show password"}
            >
              {showPassword ? <FaEyeSlash /> : <FaEye />}
            </span>
          </div>
        </label>

        <button type="submit">Update Password</button>
      </form>

      {message && (
        <div className={isError ? "error-message" : "success-message"}>
          {message}
        </div>
      )}
    </div>
  );
}
