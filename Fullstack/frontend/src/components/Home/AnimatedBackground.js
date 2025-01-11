import React from "react";
import "../../styles/Home/AnimatedBackground.css"; // Import the CSS file

const AnimatedBackground = () => {
    // Max size 180
    // Left in % of width
    // Delay until starts moving (secs)
    // Duration for it to reach top of screen (secs)
    const squareCreation = [
        { size: 100, left: 5, delay: 5, duration: 30 },
        { size: 20, left: 10, delay: 2, duration: 12 },
        { size: 30, left: 15, delay: 3, duration: 25 },
        { size: 15, left: 20, delay: 2, duration: 35 },
        { size: 80, left: 25, delay: 0, duration: 25 },
        { size: 40, left: 30, delay: 9, duration: 20 },
        { size: 150, left: 35, delay: 7, duration: 25 },
        { size: 60, left: 40, delay: 0, duration: 18 },
        { size: 25, left: 50, delay: 15, duration: 45 },
        { size: 180, left: 55, delay: 10, duration: 40 },
        { size: 60, left: 60, delay: 1, duration: 22 },
        { size: 20, left: 65, delay: 0, duration: 25 },
        { size: 20, left: 70, delay: 4, duration: 25 },
        { size: 110, left: 75, delay: 3, duration: 25 },
        { size: 70, left: 80, delay: 6, duration: 15 },
        { size: 150, left: 85, delay: 0, duration: 12 },
        { size: 50, left: 90, delay: 1, duration: 20 },
        { size: 15, left: 95, delay: 7, duration: 14 },

    ];

    return (
        <div className="holder">
            <ul className="squares">
                {squareCreation.map((square, index) => (
                    <li
                        key={index}
                        style={{
                            width: `${square.size}px`,
                            height: `${square.size}px`,
                            left: `${square.left}%`,
                            animationDelay: `${square.delay}s`,
                            animationDuration: `${square.duration}s`,
                        }}
                    />
                ))}
            </ul>
        </div>
    );
};

export default AnimatedBackground;
