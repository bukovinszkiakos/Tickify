/** @type {import('next').NextConfig} */
const nextConfig = {
  reactStrictMode: true,
  async rewrites() {
    return [
      {
        source: "/Auth/:path*",
        destination: "http://localhost:5043/Auth/:path*", 
      },
      {
        source: "/api/:path*",
        destination: "http://localhost:5043/api/:path*", 
      },
    ];
  },
};

export default nextConfig;
