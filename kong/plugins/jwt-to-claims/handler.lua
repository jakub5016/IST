local jwt_decoder = require "kong.plugins.jwt.jwt_parser"
local kong       = kong

local JwtToClaims = {
  PRIORITY = 1100,           -- run after the built-in jwt (1005)
  VERSION  = "1.0.0",
}

function JwtToClaims:access(conf)
  local auth_header = kong.request.get_header("authorization")
  if not auth_header then
    return
  end

  local token = auth_header:match("Bearer%s+(.+)")
  if not token then
    return
  end

  -- decode (the built-in JWT plugin will already have verified signature)
  local jwt, err = jwt_decoder:new(token)
  if err then
    kong.log.err("jwt-to-claims: could not decode token: ", err)
    return
  end

  local claims = jwt.claims or {}
  local headers_to_set = {
    iss                = "x-jwt-iss",
    email              = "x-jwt-email",
    is_active          = "x-jwt-is-active",
    is_confirmed_email = "x-jwt-is-confirmed-email",
    related_id         = "x-jwt-related-id",
    role               = "x-jwt-role",
    identity_confirmed = "x-jwt-identity-confirmed"
  }

  for claim_key, header_name in pairs(headers_to_set) do
    local v = claims[claim_key]
    if v ~= nil then
      kong.service.request.set_header(header_name, tostring(v))
    end
  end
end

return JwtToClaims
