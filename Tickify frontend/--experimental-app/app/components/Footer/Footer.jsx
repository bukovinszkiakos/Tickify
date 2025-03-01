import React from "react";
import Link from "next/link";
import "../../styles/Footer.css"; 

export default function Footer() {
  return (
    <footer className="footer">
      <p>© 2023 Tickify - All rights reserved.</p>
      <p>
        <Link href="/privacy">Privacy</Link>
        <span> | </span>
        <Link href="/terms">Terms</Link>
      </p>
    </footer>
  );
}
