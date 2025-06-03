/** @type {import('next').NextConfig} */
const nextConfig = {
  reactStrictMode: true,
  async rewrites() {
    return [
      {
        source: "/Auth/:path*",
        destination: "http://tickify-backend:5000/Auth/:path*",
      },
      {
        source: "/api/:path*",
        destination: "http://tickify-backend:5000/api/:path*",
      },
    ];
  },
};

export default nextConfig;

