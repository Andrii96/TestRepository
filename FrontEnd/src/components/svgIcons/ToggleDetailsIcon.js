import React from 'react'
import PropTypes from 'prop-types'

const ToggleDetailsIcon = ({
    width,
    height,
    isOpenDetails
}) => (
    <svg
        style={{transform: isOpenDetails? 'rotate(0deg)' : 'rotate(-90deg)'}}
        height={height}
        viewBox="0 0 24 24"
        width={width}
        xmlns="http://www.w3.org/2000/svg"
    >
        <path d="M7 10l5 5 5-5z"/>
        <path d="M0 0h24v24H0z" fill="none"/>
    </svg>
);

ToggleDetailsIcon.propTypes = {
    width: PropTypes.number.isRequired,
    height: PropTypes.number.isRequired,
    isOpenDetails: PropTypes.bool.isRequired
};


export default ToggleDetailsIcon