import React from 'react'
import PropTypes from 'prop-types'

const OkIcon = ({
    width,
    height
}) => (
    <svg
        fill="#fff"
        height={height}
        viewBox="0 0 24 24"
        width={width}
        xmlns="http://www.w3.org/2000/svg"
    >
        <path d="M0 0h24v24H0z" fill="none"/>
        <path d="M9 16.2L4.8 12l-1.4 1.4L9 19 21 7l-1.4-1.4L9 16.2z"/>
    </svg>
);

OkIcon.propTypes = {
    width: PropTypes.number.isRequired,
    height: PropTypes.number.isRequired
};


export default OkIcon