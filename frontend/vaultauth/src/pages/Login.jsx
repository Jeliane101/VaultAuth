import { useState } from "react";
import { useNavigate, Link } from "react-router-dom";
import "../styles/login.css";
import logo from "../assets/vaultauth-logo1.png";
import { FaEye, FaEyeSlash } from "react-icons/fa";

export default function LoginPage() {
  const [formData, setFormData] = useState({ email: "", password: "" });
  const [message, setMessage] = useState("");
  const [isError, setIsError] = useState(false);
  const [showPassword, setShowPassword] = useState(false);
  const navigate = useNavigate();

  const handleChange = (e) => {
    const { name, value } = e.target;
    setFormData({ ...formData, [name]: value });
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    try {
      const response = await fetch("http://localhost:5000/api/auth/login", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(formData)
      });

      const data = await response.json();

      if (response.ok) {
        //Store tokens in localStorage
        localStorage.setItem("accessToken", data.accessToken);
        localStorage.setItem("refreshToken", data.refreshToken);

        setIsError(false);
        setMessage("Login successful! Redirecting...");
        setTimeout(() => navigate("/dashboard"), 1500);
      } else {
        setIsError(true);

        if (data.lockoutEnd) {
          const lockoutEnd = new Date(data.lockoutEnd);
          const now = new Date();
          const diffMinutes = Math.ceil((lockoutEnd - now) / 60000);

          setMessage(
            `Account is temporarily locked. Try again in ${diffMinutes} minute(s).`
          );
        } else {
          setMessage(data.message || "Invalid email or password.");
        }

        // Clear fields on failure
        setFormData({ email: "", password: "" });
      }
    } catch {
      setIsError(true);
      setMessage("Something went wrong. Please try again later.");
      setFormData({ ...formData, password: "" });
    }
  };

  return (
    <div className="login-container">
      <img src={logo} alt="VaultAuth Logo" className="logo" />
      <h1>Welcome to VaultAuth</h1>
      <h2>Please login</h2>

      <form onSubmit={handleSubmit}>
        <label>
          Email <span className="required">*</span>
          <input
            type="email"
            name="email"
            placeholder="Email"
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

        <button type="submit">Login</button>
      </form>

      {message && (
        <div className={isError ? "error-message" : "success-message"}>
          {message}
        </div>
      )}

      <div className="links">
        <p>
          Don’t have an account? <Link to="/register">Sign up</Link>
        </p>
        <p>
          <Link to="/forgot-password">Forgot password?</Link>
        </p>
      </div>
    </div>
  );
}
